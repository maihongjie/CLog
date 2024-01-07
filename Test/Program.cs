using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.IO;
using CLog;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            CLog.CLogger.Instance.Init("Test", CLogger.Level.Info, CLogger.Level.Info);
            LogWriteTest();
        }
        static void LogWriteTest()
        {
            for (int i = 0; i < 5; i++)
            {
                var _thread = new Thread(Start);
                _thread.IsBackground = true;
                _thread.Start(i);
            }
            Console.ReadLine();

        }
        static void Start(object tag)
        {
            for (int i = 0; i < 20000; i++)
            {
                CLog.CLogger.Instance.INFO(i + "测试测试测试测试测试");
                CLog.CLogger.Instance.WARN(i + "测试测试测试测试测试");
                CLog.CLogger.Instance.ERROR(i + "测试测试测试测试测试");
                CLog.CLogger.Instance.EXCEPTION(i + "测试测试测试测试测试");
            }
        }

    }
}
