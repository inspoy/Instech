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
        public string Module;
        public string Content;
        public LogLevel Level;
        public StackTrace StackTrace;

        public void OnActivate()
        {
        }

        public void OnRecycle()
        {
            Module = string.Empty;
            Content = string.Empty;
            Level = LogLevel.Off;
            StackTrace = null;
        }

        #region IDisposable Support
        private bool _disposedValue; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
            {
                return;
            }
            if (disposing)
            {
                OnRecycle();
            }
            _disposedValue = true;
        }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
        }
        #endregion
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

        protected override void Init()
        {
            Logger.OnLog += OnHandleLog;

            // 创建文件
            var timeNow = DateTime.Now;
            var folderPath = Application.persistentDataPath + "/GameLog";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var path = $"{folderPath}/GameLog{timeNow:yyyyMMddHHmmss}.txt";
            _logFileInfo = new FileInfo(path);
            var sw = _logFileInfo.CreateText();
            sw.WriteLine("[{0}] - {1:yyyy/MM/dd HH:mm:ss}", Application.productName, timeNow);
            sw.Close();

            // 启动写文件的线程
            _writerThread = new Thread(FileWriterMain);
            _writerThread.Start();
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
                    lock (_locker)
                    {
                        if (_logItems.Count == 0)
                        {
                            continue;
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
                            var timeNow = DateTime.Now;
                            var logStr = $"{timeNow:yyyy/MM/dd HH:mm:ss}-[{item.Level.ToString().ToUpper().PadLeft(7)}][{item.Module}]{item.Content}";
                            if (item.Level == LogLevel.Exception || item.Level == LogLevel.Assert || item.Level == LogLevel.Error)
                            {
                                logStr += "\n" + item.StackTrace;
                            }
                            sw.WriteLine(logStr);
                            sw.Close();
                            ObjectPool<LogToFileItem>.Instance.Recycle(_logItemsForWriter.Dequeue());
                        }
                        catch (IOException)
                        {
                            // 文件被占用，等待
                            Thread.Sleep(100);
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // 线程正常退出
                var sw = _logFileInfo.AppendText();
                var timeNow = DateTime.Now;
                sw.WriteLine($"{timeNow:yyyy/MM/dd HH:mm:ss}-Application Quit");
                sw.Close();
            }
            catch (Exception e)
            {
                // 意外错误
                Logger.LogWarning(LogModule.Framework, "日志线程意外退出: " + e.Message);
            }
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
