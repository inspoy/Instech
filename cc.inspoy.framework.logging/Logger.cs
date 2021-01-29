// == Inspoy Technology ==
// Assembly: Instech.Framework.Logging
// FileName: Logger.cs
// Created on 2018/05/02 by inspoy
// All rights reserved.

using System;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

// 需要给程序集定义以下符号，对应的接口才会被调用
// INSTECH_LOGGER_ENABLE_NORMAL: 启用Print, LogDebug, LogVerbose, LogInfo, LogWarning
// INSTECH_LOGGER_ENABLE_ERROR: 启用LogError
// INSTECH_LOGGER_ENABLE_EXCEPTION: 启用LogException, Assert
// 另外还有用于Profiling的：INSTECH_LOGGER_PROFILING

namespace Instech.Framework.Logging
{
    /// <summary>
    /// 输出日志等级
    /// </summary>
    [Flags]
    public enum LogLevels
    {
        /// <summary>
        /// 关闭所有日志
        /// </summary>
        None = 0,

        /// <summary>
        /// 异常
        /// </summary>
        Exception = 0x1,

        /// <summary>
        /// 断言
        /// </summary>
        Assert = 0x2,

        /// <summary>
        /// 错误
        /// </summary>
        Error = 0x4,

        /// <summary>
        /// 警告
        /// </summary>
        Warning = 0x8,

        /// <summary>
        /// 信息类日志
        /// </summary>
        Info = 0x10,

        /// <summary>
        /// 不重要的冗余信息
        /// </summary>
        Verbose = 0x20,

        /// <summary>
        /// 调试信息
        /// </summary>
        Debug = 0x40,

        /// <summary>
        /// 输出所有日志
        /// </summary>
        All = 0x80 - 1
    }

    public sealed class AssertionException : Exception
    {
        public AssertionException(string msg) : base("Assert failed: " + msg)
        {
        }
    }

    /// <summary>
    /// 封装Unity的Debug.Log方法族，实现灵活配置
    /// </summary>
    public static class Logger
    {
        #region Public Properties & Fields

        /// <summary>
        /// 用于日志的回调委托
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="content">日志内容</param>
        /// <param name="level">日志等级</param>
        /// <param name="stackTrace">堆栈信息</param>
        public delegate void LogCallback(string module, string content, LogLevels level, StackTrace stackTrace);

        public static ulong CurDebugFlags = 1;

        /// <summary>
        /// 输出的日志等级，包含等级的日志将会被记录
        /// </summary>
        public static LogLevels LogLevels { get; set; } = LogLevels.All;

        /// <summary>
        /// 关闭Warning及以下级别的调用栈信息，为了输出Log时尽量少一些消耗（推荐）；
        /// </summary>
        public static bool DisableNormalStackTrace { get; set; } = true;

        /// <summary>
        /// 关闭Error和Assert级别的调用栈信息，为了减少消耗（不推荐）；
        /// 异常中带的StrackTrace不受影响
        /// </summary>
        public static bool DisableErrorStackTrace { get; set; } = false;

        /// <summary>
        /// 屏蔽Unity的Debug.Log
        /// </summary>
        public static bool DisableUnityLog { get; set; } = false;

        /// <summary>
        /// 当日志发生时触发
        /// </summary>
        public static event LogCallback OnLog;

        #endregion

#if UNITY_EDITOR
        private static readonly object PrintLocker = new object();
#endif

#if INSTECH_LOGGER_PROFILING
        private const string ProfilingLabel = "Logger.LogImpl()";
#endif

        /// <summary>
        /// 输出临时调试信息，线程安全
        /// </summary>
        [Conditional("INSTECH_LOGGER_ENABLE_NORMAL")]
        public static void Print(string msg)
        {
#if UNITY_EDITOR
            lock (PrintLocker)
            {
                LogDebug(null, 1, msg);
            }
#else
            throw new NotSupportedException("Print() is only for dev, it can only be used in editor");
#endif
        }

        /// <summary>
        /// 输出调试信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="flag"></param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        [Conditional("INSTECH_LOGGER_ENABLE_NORMAL")]
        public static void LogDebug(string module, ulong flag, string message, Object context = null)
        {
            if ((CurDebugFlags & flag) > 0)
            {
                LogImpl(module, LogLevels.Debug, message, context);
            }
        }

        /// <summary>
        /// 输出不重要的冗余信息
        /// </summary>
        /// <param name="module"></param>
        /// <param name="message"></param>
        /// <param name="context"></param>
        [Conditional("INSTECH_LOGGER_ENABLE_NORMAL")]
        public static void LogVerbose(string module, string message, Object context = null)
        {
            LogImpl(module, LogLevels.Verbose, message, context);
        }

        /// <summary>
        /// 输出常规信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        [Conditional("INSTECH_LOGGER_ENABLE_NORMAL")]
        public static void LogInfo(string module, string message, Object context = null)
        {
            LogImpl(module, LogLevels.Info, message, context);
        }

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        [Conditional("INSTECH_LOGGER_ENABLE_NORMAL")]
        public static void LogWarning(string module, string message, Object context = null)
        {
            LogImpl(module, LogLevels.Warning, message, context);
        }

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        [Conditional("INSTECH_LOGGER_ENABLE_ERROR")]
        public static void LogError(string module, string message, Object context = null)
        {
            LogImpl(module, LogLevels.Error, message, context);
        }

        /// <summary>
        /// 输出异常信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="ex">发生的异常</param>
        /// <param name="context">上下文信息</param>
        [Conditional("INSTECH_LOGGER_ENABLE_EXCEPTION")]
        public static void LogException(string module, Exception ex, Object context = null)
        {
            if (ex == null)
            {
                return;
            }

            LogImpl(module, LogLevels.Exception, null, context, ex);
        }

        /// <summary>
        /// 执行断言
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="condition">断言条件</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        [Conditional("INSTECH_LOGGER_ENABLE_EXCEPTION")]
        public static void Assert(string module, bool condition, string message, Object context = null)
        {
            if (!condition)
            {
                LogImpl(module, LogLevels.Assert, message, context);
                throw new AssertionException(message);
            }
        }

        /// <summary>
        /// 输出日志
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="level">日志等级</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        /// <param name="ex">异常信息</param>
        private static void LogImpl(string module, LogLevels level, string message, Object context, Exception ex = null)
        {
            if ((level & LogLevels) == 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(message) && level != LogLevels.Exception)
            {
                return;
            }

#if INSTECH_LOGGER_PROFILING
            UnityEngine.Profiling.Profiler.BeginSample(ProfilingLabel);
#endif

            if (string.IsNullOrWhiteSpace(module))
            {
                module = "Default";
            }

            var needStackTrace = false;
            needStackTrace |= (level & (LogLevels.Assert | LogLevels.Error)) > 0 && !DisableErrorStackTrace;
            needStackTrace |= (level & (LogLevels.Warning | LogLevels.Info | LogLevels.Verbose | LogLevels.Debug)) > 0 && !DisableNormalStackTrace;
            var stackTrace = needStackTrace ? new StackTrace(true) : null;
            if (level == LogLevels.Exception && ex != null)
            {
                var exMessage = new StringBuilder();
                exMessage.Append("[!]异常信息: ");
                exMessage.Append(ex.Message);
                exMessage.Append('\n');
                exMessage.Append("[!]异常类型: ");
                exMessage.Append(ex.GetType());
                exMessage.Append('\n');
                exMessage.Append("[!]调用栈:\n ");
                exMessage.Append(ex.StackTrace);
                exMessage.Append("\n=============");
                OnLog?.Invoke(module, exMessage.ToString(), LogLevels.Exception, stackTrace);
                if (!DisableUnityLog)
                {
                    Debug.LogException(ex, context);
                    Debug.Log(exMessage, context);
                }

#if INSTECH_LOGGER_PROFILING
                UnityEngine.Profiling.Profiler.EndSample();
#endif
                return;
            }

            OnLog?.Invoke(module, message, level, stackTrace);
            if (Application.platform != RuntimePlatform.WindowsEditor || DisableUnityLog)
            {
                // 编辑器下且没有显式禁用，才会调用Unity的Log
                // 非编辑器下，所有log走LogToFile输出到指定文件。
#if INSTECH_LOGGER_PROFILING
                UnityEngine.Profiling.Profiler.EndSample();
#endif
                return;
            }

            var msg = $"[{level}][{module}] - {message}";
            switch (level)
            {
                case LogLevels.Assert:
                    Debug.LogAssertion(msg, context);
                    break;
                case LogLevels.Exception:
                case LogLevels.Error:
                    Debug.LogError(msg, context);
                    break;
                case LogLevels.Warning:
                    Debug.LogWarning(msg, context);
                    break;
                case LogLevels.Info:
                case LogLevels.Verbose:
                case LogLevels.Debug:
                    Debug.Log(msg, context);
                    break;
            }
#if INSTECH_LOGGER_PROFILING
                UnityEngine.Profiling.Profiler.EndSample();
#endif
        }
    }
}
