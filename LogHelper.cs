using System;

namespace CLog
{
    /// <summary>
    /// 该插件的错误日志处理类
    /// </summary>
    internal class Debug
    {
        internal enum Level
        {
            None,
            Error,
            Warn,
            Info
        }

        internal static Level internalLogLevel = Level.Info;

        internal static void SetMaxLevleToHandle(string level)
        {
            switch (level)
            {
                case "error":
                    internalLogLevel = Level.Error;
                    break;
                case "warn":
                    internalLogLevel = Level.Warn;
                    break;
                case "info":
                    internalLogLevel = Level.Info;
                    break;
                default:
                    internalLogLevel = Level.None;
                    break;
            }
        }

        internal static void Log(string context)
        {
            if (internalLogLevel < Level.Info)
                return;
            Console.WriteLine(context);
        }

        internal static void Warn(string context)
        {
            if (internalLogLevel < Level.Warn)
                return;
            Console.WriteLine(context);
        }

        internal static void Error(string context)
        {
            if (internalLogLevel < Level.Error)
                return;
            Console.WriteLine(context);
        }
    }
}
