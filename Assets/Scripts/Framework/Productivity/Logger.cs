/**
 * == Inspoy Technology ==
 * Assembly: Framework
 * FileName: Logger.cs
 * Created on 2018/05/02 by inspoy
 * All rights reserved.
 */

using System;
using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Instech.Framework
{
    /// <summary>
    /// 输出日志等级
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        ///     关闭所有日志
        /// </summary>
        Off = 0,

        /// <summary>
        ///     异常
        /// </summary>
        Exception = 50,

        /// <summary>
        ///     断言
        /// </summary>
        Assert = 100,

        /// <summary>
        ///     错误
        /// </summary>
        Error = 200,

        /// <summary>
        ///     警告
        /// </summary>
        Warning = 300,

        /// <summary>
        ///     信息类日志
        /// </summary>
        Info = 400,

        /// <summary>
        ///     调试信息
        /// </summary>
        Debug = 500,

        /// <summary>
        ///     输出所有日志
        /// </summary>
        All = 999
    }

    /// <summary>
    /// 封装Unity的Debug.Log方法族，实现灵活配置
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// 用于日志的回调委托
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="content">日志内容</param>
        /// <param name="level">日志等级</param>
        /// <param name="stackTrace">堆栈信息</param>
        public delegate void LogCallback(string module, string content, LogLevel level, StackTrace stackTrace);

        /// <summary>
        ///     输出的日志等级，高于或等于该等级的日志将会被记录
        /// </summary>
        public static LogLevel LogLevel = LogLevel.All;

        /// <summary>
        ///     当日志发生时触发
        /// </summary>
        public static event LogCallback OnLog;

        /// <summary>
        /// 输出调试信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        public static void LogDebug(string module, string message, Object context = null)
        {
            Log(module, LogLevel.Debug, message, context);
        }

        /// <summary>
        /// 输出常规信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        public static void LogInfo(string module, string message, Object context = null)
        {
            Log(module, LogLevel.Info, message, context);
        }

        /// <summary>
        /// 输出警告信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        public static void LogWarning(string module, string message, Object context = null)
        {
            Log(module, LogLevel.Warning, message, context);
        }

        /// <summary>
        /// 输出错误信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        public static void LogError(string module, string message, Object context = null)
        {
            Log(module, LogLevel.Error, message, context);
        }

        /// <summary>
        /// 输出异常信息
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="ex">发生的异常</param>
        /// <param name="context">上下文信息</param>
        public static void LogException(string module, Exception ex, Object context = null)
        {
            if (ex == null)
            {
                return;
            }
            Log(module, LogLevel.Exception, null, context, ex);
        }

        /// <summary>
        /// 执行断言
        /// </summary>
        /// <param name="module">日志所属模块</param>
        /// <param name="condition">断言条件</param>
        /// <param name="message">日志信息</param>
        /// <param name="context">上下文信息</param>
        public static void Assert(string module, bool condition, string message, Object context = null)
        {
            if (!condition)
            {
                Log(module, LogLevel.Assert, message, context);
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
        private static void Log(string module, LogLevel level, string message, Object context, Exception ex = null)
        {
            if (level > LogLevel)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(module))
            {
                module = "Default";
            }
            var stackTrace = new StackTrace(true);
            if (level == LogLevel.Exception && ex != null)
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
                OnLog?.Invoke(module, exMessage.ToString(), LogLevel.Exception, stackTrace);
                Debug.LogException(ex, context);
                return;
            }
            OnLog?.Invoke(module, message, level, stackTrace);
#if UNITY_EDITOR
            switch (level)
            {
                case LogLevel.Assert:
                    Debug.LogAssertion(message, context);
                    break;
                case LogLevel.Exception:
                case LogLevel.Error:
                    Debug.LogError(message, context);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message, context);
                    break;
                case LogLevel.Info:
                case LogLevel.Debug:
                    Debug.Log(message, context);
                    break;
            }
#endif
        }
    }
}
