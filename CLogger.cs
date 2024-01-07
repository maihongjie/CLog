using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace CLog
{
    public class CLogger
    {
        internal const string SDK_VERSION = "1.0.0";
        public enum Level
        {
            None = 0,
            Exception,
            Error,
            Warn,
            Info
        }

        private CLogger() { }
        public static readonly CLogger Instance = new CLogger();

        private const int MAX_QUEUE_COUNT = int.MaxValue;    // 日志缓存队列最大缓存数量
        internal string m_basePath = "CLog";


        //日志写本地文件相关参数
        private const int FILE_MAX_BYTE_SIZE = 1024 * 64;   // 64KB,当缓存日志达到多少byte时写入文件
        internal Level m_fileLogLevel = Level.None;   // 本地日志收集等级
        private ConcurrentQueue<LogItem> m_fileLogQueue = new ConcurrentQueue<LogItem>();
        private StringBuilder m_fileLogBuffer = new StringBuilder(FILE_MAX_BYTE_SIZE);
        private Thread m_fileLogThread = null;
        private StreamWriter m_fileWriter = null;

        //日志上报服务器相关参数
        internal string uploadInfoAddress = "http://127.0.0.1:5000/info";// 测试环境：10.11.81.87:8686， 正式环境：logapi.testplus.cn
        internal string uploadLogAddress = "http://127.0.0.1:5000/logs";
        private const int UPLOAD_MAX_BYTE_SIZE = 1024 * 64;    // 64KB,当缓存日志达到多少byte时上报服务器
        internal Level m_uploadLogLevel = Level.None;   // 上报服务器日志收集等级
        private ConcurrentQueue<LogItem> m_uploadLogQueue = new ConcurrentQueue<LogItem>();
        private StringBuilder m_uploadBuffer = new StringBuilder(UPLOAD_MAX_BYTE_SIZE);
        private Thread m_uploadThread = null;


        private bool isInit = false;    //判断是否成功初始化
        internal bool m_enableSave = false;
        internal bool m_enableUpload = false;

        
        internal string m_appKey = string.Empty;
        private string m_ipAddress = "null";
        private string m_projectVersion = "null";
        private string m_channeId = "null";
        private string m_branchTag = "null";
        private string m_playerAccount = "null";
        private string m_mapName = "null";
        private string m_uniteDeviceId = "null";

        internal string m_uuid = string.Empty;
        internal string m_sessionId = string.Empty;


        public void EnableSave(bool value) { m_enableSave = value; }

        public void EnableUpload(bool value) { m_enableUpload = value; }

        public void SetFileLogLevel(Level level) { m_fileLogLevel = level; }

        public void SetUploadLogLevel(Level level) { m_uploadLogLevel = level; }


        // 项目版本号
        public string ProjectVersion
        {
            get { return m_projectVersion; }
            set { m_projectVersion = value; }
        }

        // 渠道id
        public string ChanneId
        {
            get { return m_channeId; }
            set { m_channeId = value; }
        }

        // 主干/分支
        public string BranchTag
        {
            get { return m_branchTag; }
            set { m_branchTag = value; }
        }

        // 游戏账号
        public string PlayerAccount
        {
            get { return m_playerAccount; }
            set { m_playerAccount = value; }
        }

        // 当前地图名称
        public string MapName
        {
            get { return m_mapName; }
            set { m_mapName = value; }
        }

        // 设备IP
        internal string IpAddress
        {
            get
            {
                if (string.IsNullOrEmpty(m_ipAddress))
                    m_ipAddress = GetAddressIP();

                return m_ipAddress;
            }
            set { m_ipAddress = value; }
        }



        // 初始化
        public void Init(string appKey, Level fileLogLevel = Level.Info, Level uploadLogLevel = Level.Info)
        {
            if (isInit) return;
            if (string.IsNullOrEmpty(appKey))
            {
                Debug.Error("[CLog] Init failed, AppKey is null!");
                return;
            }
            m_appKey = appKey;
            m_fileLogLevel = fileLogLevel;
            m_uploadLogLevel = uploadLogLevel;
            m_enableSave = true;
            m_enableUpload = true;
            isInit = true;

            StartFileLogThread();
            StartUploadThread();
        }

        // 反初始化
        public void DeInit()
        {
            m_enableSave = false;
            m_enableUpload = false;
            DestroyFileLogThread();
            DestroyUploadThread();
        }

        public void INFO(string context)
        {
            Log(Level.Info, context);
        }
        public void WARN(string context)
        {
            Log(Level.Warn, context);
        }
        public void ERROR(string context)
        {
            Log(Level.Error, context);
        }
        public void EXCEPTION(string context)
        {
            Log(Level.Exception, context);
        }

        private void Log(Level level, string context)
        {
            if (!isInit || (!m_enableSave && !m_enableUpload) || (level > m_fileLogLevel && level > m_uploadLogLevel))
                return;
            
            LogItem logItem = new LogItem() {
                context = context,
                level = level,
            };

            if (m_enableSave && level <= m_fileLogLevel && m_fileLogQueue.Count < MAX_QUEUE_COUNT)
            {
                m_fileLogQueue.Enqueue(logItem);
            }

            if (m_enableUpload && level <= m_uploadLogLevel && m_uploadLogQueue.Count < MAX_QUEUE_COUNT)
            {
                m_uploadLogQueue.Enqueue(logItem);
            }
        }

        private void StartFileLogThread()
        {
            if (m_fileLogThread == null)
            {
                m_fileLogThread = new Thread(FileLogHandle);
                m_fileLogThread.IsBackground = true;
                m_fileLogThread.Start();
            }
        }

        private void DestroyFileLogThread()
        {
            if (m_fileLogThread != null)
            {
                m_fileLogThread.Join();
                m_fileLogThread = null;
            }
        }

        private void StartUploadThread()
        {
            if (m_uploadThread == null)
            {
                m_uploadThread = new Thread(UploadHandle);
                m_uploadThread.IsBackground = true;
                m_uploadThread.Start();
            }
        }

        private void DestroyUploadThread()
        {
            if (m_uploadThread != null)
            {
                m_uploadThread.Join();
                m_uploadThread = null;
            }
        }

        private void FileLogHandle()
        {
            if (!m_enableSave) return;
            Debug.Log("[CLog] FileLogHandle Start");

            DateTime startTime = DateTime.Now;
            int second = 0;
            LogItem logItem = new LogItem();
            while (m_enableSave)
            {
                if (m_fileLogQueue.Count > 0)
                {
                    m_fileLogQueue.TryDequeue(out logItem);
                    AppendFileLogString(logItem);

                    second = (DateTime.Now - startTime).Seconds;
                    if (m_fileLogBuffer.Length > FILE_MAX_BYTE_SIZE || second > 5)
                    {
                        WriteFile();
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void UploadHandle()
        {
            if (!m_enableUpload) return;
            Debug.Log("[CLog] UploadHandle Start");
            bool result = UploadBaseInfo();
            if (!result) return;

            DateTime startTime = DateTime.Now;
            int second = 0;
            LogItem logItem = new LogItem();
            while (m_enableUpload)
            {
                if (m_uploadLogQueue.Count > 0)
                {
                    m_uploadLogQueue.TryDequeue(out logItem);
                    AppendUploadString(logItem);

                    second = (DateTime.Now - startTime).Seconds;
                    if (m_uploadBuffer.Length > UPLOAD_MAX_BYTE_SIZE || second > 5)
                    {
                        UploadServer();
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private bool UploadBaseInfo()
        {
            __InitUUID();
            __InitSessionId();

            try
            {
                // 发送baseInfo数据
                Debug.Log(JsonSerializer.Serialize(new BaseData()));
                string msg = UploadUtil.Post(uploadInfoAddress, JsonSerializer.Serialize(new BaseData()), false);
                Debug.Log($"[CLog] Send baseInfo response:{msg}");
                ResponseData response = JsonSerializer.Deserialize<ResponseData>(msg);
                m_enableUpload = response.enable.Equals("true");
                m_uploadLogLevel = __StringToLevel(response.log_level);
            }
            catch (Exception ex)
            {
                Debug.Error("[CLog] Upload BaseInfo failed, error=" + ex.ToString());
                return false;
            }

            return m_enableUpload;
        }

        /// <summary>
        /// KGLog随机生成的ID
        /// </summary>
        private void __InitUUID()
        {
            if (string.IsNullOrEmpty(m_uuid))
            {
                m_uuid = Guid.NewGuid().ToString("N");
            }
        }
        

        /// <summary>
        /// Session ID 是每次启动的时候随机生成
        /// 用于定位这一次的报错
        /// </summary>
        private void __InitSessionId()
        {
            // 初始化SessionId
            m_sessionId = Guid.NewGuid().ToString("N"); //随机生成32位数
        }

        private void InitConfig()
        {
            Config config = ConfigManager.Instance.Deserializer();
            Debug.internalLogLevel = config.internalLogLevel;
            Instance.uploadInfoAddress = config.uploadInfoAddress;
            Instance.uploadLogAddress = config.uploadLogAddress;
            Instance.m_enableSave = config.enableSave;
            Instance.m_enableUpload = config.enableUpload;
            Instance.m_fileLogLevel = config.fileLogLevel;
            Instance.m_uploadLogLevel = config.uploadLogLevel;
        }

        private void WriteFile()
        {
            try
            {
                string logPath = Path.Combine(m_basePath, "logs");
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                string logFile = Path.Combine(logPath, string.Format("{0:yyyy_MM_dd_HH_mm_ss}.log", DateTime.Now));
                if (m_fileWriter == null)
                    m_fileWriter = new StreamWriter(logFile);

                m_fileWriter.Write(m_fileLogBuffer);
            }
            catch (Exception ex)
            {
                Debug.Error("[CLog] Save log filed, error=" + ex.ToString());
            }
            finally
            {
                m_fileWriter.Flush();
                m_fileLogBuffer.Clear();
            }
        }

        private void UploadServer()
        {
            try
            {
                string response = UploadUtil.Post(uploadLogAddress, m_uploadBuffer.ToString(), true);
                if (!string.IsNullOrEmpty(response))
                {
                    ResponseData res = JsonSerializer.Deserialize<ResponseData>(response);
                    if (res.ret == 0 && res.msg == "SUCCESS")
                        Debug.Log($"[CLog] Upload successful, uploadBuffer lenght: {m_uploadBuffer.Length}");
                    else
                        Debug.Error($"[CLog] Upload failed, ret={res.ret}, msg={res.msg}, uploadBuffer lenght: {m_uploadBuffer.Length}");
                }
            }
            catch (Exception ex)
            {
                Debug.Error("[KGlog] Upload exception : " + ex);
            }
            finally
            {
                m_uploadBuffer.Clear();
            }

        }

        private void AppendFileLogString(LogItem logItem)
        {
            m_fileLogBuffer.AppendFormat("{0} {1} {2}{3}", __TimeStr(), __LevelToString(logItem.level), logItem.context, Environment.NewLine);
        }

        private void AppendUploadString(LogItem logItem)
        {
            if (m_uploadBuffer.Length == 0)
            {
                m_uploadBuffer.Append("{");
                m_uploadBuffer.AppendFormat("\"appkey\":\"{0}\",", m_appKey);
                m_uploadBuffer.AppendFormat("\"ip\":\"{0}\",", m_ipAddress);
                m_uploadBuffer.AppendFormat("\"project_version\":\"{0}\",", m_projectVersion);
                m_uploadBuffer.AppendFormat("\"session_id\":\"{0}\",", m_sessionId);
                m_uploadBuffer.AppendFormat("\"uuid\":\"{0}\",", m_uuid);
                m_uploadBuffer.Append("\"logArray\":[");
            }
            else
            {
                m_uploadBuffer.Length -= 2;
                m_uploadBuffer.Append(",");
            }
            //单个日志信息
            m_uploadBuffer.Append("{");
            m_uploadBuffer.AppendFormat("\"time\":\"{0}\",", __UTCTimestr());
            m_uploadBuffer.AppendFormat("\"level\":\"{0}\",", logItem.level.ToString());
            m_uploadBuffer.AppendFormat("\"map_name\":\"{0}\",", m_mapName);
            m_uploadBuffer.Append("\"log_string\":\"");

            AppendEscapes(m_uploadBuffer, logItem.context);

            m_uploadBuffer.Append("\"}");
            m_uploadBuffer.Append("]}");
        }

        private StringBuilder AppendEscapes(StringBuilder stringBuilder, string value)
        {
            if (stringBuilder == null)
                return null;

            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\\':
                        stringBuilder.Append(@"\\");
                        break;
                    case '\n':
                        stringBuilder.Append(@"\n");
                        break;
                    case '\r':
                        stringBuilder.Append(@"\r");
                        break;
                    case '\t':
                        stringBuilder.Append(@"\t");
                        break;
                    case '\v':
                        stringBuilder.Append(@"\v");
                        break;
                    case '\"':
                        stringBuilder.Append("\\\"");
                        break;
                    default:
                        stringBuilder.Append(ch);
                        break;
                }
            }
            return stringBuilder;
        }

        #region helper function 
        private string __TimeStr()
        {
            return DateTime.Now.ToString("yyyy/MM/dd_HH:mm:ss,fff");
        }

        private string __UTCTimestr()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");
        }

        private string __LevelToString(Level level)
        {
            switch (level)
            {
                case Level.Exception:
                    return "EXCEPTION";
                case Level.Error:
                    return "ERROR";
                case Level.Warn:
                    return "WARN";
                case Level.Info:
                    return "INFO";
                default:
                    return "_____ ";
            }
        }

        private Level __StringToLevel(string level)
        {
            switch (level)
            {
                case "exception":
                    return Level.Exception;
                case "error":
                    return Level.Error;
                case "warn":
                    return Level.Warn;
                case "info":
                    return Level.Info;
                default:
                    return Level.None;
            }
        }

        private string GetAddressIP()
        {
            var addressList = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
            foreach (var ipAddress in addressList)
            {
                if (ipAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    return ipAddress.ToString();
                }
            }
            return "127.0.0.1";
        }

        #endregion

    }

    class LogItem
    {
        public string context;
        public CLogger.Level level;
    }


}
