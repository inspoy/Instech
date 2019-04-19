/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: LogToFile.cs
 * Created on 2018/05/09 by inspoy
 * All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

namespace Instech.Framework
{
    /// <summary>
    /// 日志项
    /// </summary>
    public class LogToFileItem : IPoolable
    {
        public string Module { get; set; }
        public string Content { get; set; }
        public LogLevel Level { get; set; }
        public StackTrace StackTrace { get; set; }

        public void OnActivate()
        {
            // do nothing
        }

        public void OnRecycle()
        {
            Module = string.Empty;
            Content = string.Empty;
            Level = LogLevel.Off;
            StackTrace = null;
        }

        public void Dispose()
        {
            // do nothing
        }
    }

    /// <summary>
    /// 把日志输出到文件，子线程负责文件IO操作，不阻塞主线程
    /// </summary>
    public class LogToFile : Singleton<LogToFile>
    {
        private readonly object _locker = new object();
        private Queue<LogToFileItem> _logItems = new Queue<LogToFileItem>();
        private Queue<LogToFileItem> _logItemsForWriter = new Queue<LogToFileItem>();
        private Thread _writerThread;
        private FileInfo _logFileInfo;
        private FileInfo _latestLogFileInfo;

        protected override void Init()
        {
            // 创建文件
            var timeNow = DateTime.Now;
            var folderPath = Application.persistentDataPath + "/GameLog";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var path = $"{folderPath}/GameLog{timeNow:yyyyMMddHHmmss}.log";
            _logFileInfo = new FileInfo(path);
            var sw = _logFileInfo.CreateText();
            sw.WriteLine("[{0}] - {1:yyyy/MM/dd HH:mm:ss}", Application.productName, timeNow);
            sw.Close();
            _latestLogFileInfo = new FileInfo($"{folderPath}/LatestGameLog.log");
            sw = _latestLogFileInfo.CreateText();
            sw.WriteLine("[{0}] - {1:yyyy/MM/dd HH:mm:ss}", Application.productName, timeNow);
            sw.Close();
            Logger.LogInfo(LogModule.Framework, "日志文件路径: " + path);

            // 启动写文件的线程
            _writerThread = new Thread(FileWriterMain);
            _writerThread.Start();
            Logger.OnLog += OnHandleLog;
            Logger.LogInfo(LogModule.Framework, "写日志线程已启动");
        }

        protected override void Uninit()
        {
            Logger.OnLog -= OnHandleLog;
            _writerThread.Abort();
            _writerThread = null;
        }

        /// <summary>
        /// 写文件的线程
        /// </summary>
        private void FileWriterMain()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(100);
                    WriteAllLogItems();
                }
            }
            catch (ThreadAbortException)
            {
                // 线程正常退出
                WriteAllLogItems();
                var sw = _logFileInfo.AppendText();
                var timeNow = DateTime.Now;
                sw.WriteLine($"{timeNow:yyyy/MM/dd HH:mm:ss}-Application Quit");
                sw.Close();
                sw = _latestLogFileInfo.AppendText();
                sw.WriteLine($"{timeNow:yyyy/MM/dd HH:mm:ss}-Application Quit");
                sw.Close();
            }
            catch (Exception e)
            {
                // 意外错误
                Logger.LogWarning(LogModule.Framework, "日志线程意外退出: " + e.Message);
            }
        }

        private void WriteAllLogItems()
        {
            lock (_locker)
            {
                if (_logItems.Count == 0)
                {
                    return;
                }
                var t = _logItems;
                _logItems = _logItemsForWriter;
                _logItemsForWriter = t;
            }
            while (_logItemsForWriter.Count > 0)
            {
                try
                {
                    var sw = _logFileInfo.AppendText();
                    var item = _logItemsForWriter.Peek();
                    WriteContentToFile(sw, item);
                    sw = _latestLogFileInfo.AppendText();
                    WriteContentToFile(sw, item);
                    ObjectPool<LogToFileItem>.Instance.Recycle(_logItemsForWriter.Dequeue());
                }
                catch (IOException)
                {
                    // 文件被占用，等待
                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// 把内容写到文件里，子线程调用
        /// </summary>
        /// <param name="sw"></param>
        /// <param name="item"></param>
        private static void WriteContentToFile(StreamWriter sw, LogToFileItem item)
        {
            var timeNow = DateTime.Now;
            var logStr = $"{timeNow:yyyy/MM/dd HH:mm:ss}-[{item.Level.ToString().ToUpper().PadLeft(7)}][{item.Module.PadLeft(10)}]-{item.Content}";
            if (item.Level == LogLevel.Exception || item.Level == LogLevel.Assert || item.Level == LogLevel.Error)
            {
                if (item.StackTrace != null)
                {
                    logStr += "\n" + item.StackTrace;
                }
                else
                {
                    logStr += "\nStackTraceDisabled\n";
                }
            }
            logStr += "\n";
            sw.Write(logStr);
            sw.Close();
        }

        private void OnHandleLog(string module, string content, LogLevel level, StackTrace stackTrace)
        {
            var logItem = ObjectPool<LogToFileItem>.Instance.Get();
            logItem.Module = module;
            logItem.Content = content;
            logItem.Level = level;
            logItem.StackTrace = stackTrace;
            lock (_locker)
            {
                _logItems.Enqueue(logItem);
            }
        }
    }
}
