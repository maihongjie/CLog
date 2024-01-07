using System;
using System.Collections.Generic;

namespace CLog
{

    [Serializable]
    public class LogData
    {
        public string appkey;   // 项目ID，由质量平台生成 初始化接口传入
        public string ip;
        public string project_version;  // 版本号 初始化接口传入
        public string uuid; // UUID设备唯一标识
        public string session_id;   // 每次启动生成
        public List<LogInfo> logArray;

        public LogData()
        {
            logArray = new List<LogInfo>();

            session_id = CLogger.Instance.m_sessionId;
            uuid = CLogger.Instance.m_uuid;
            ip = CLogger.Instance.IpAddress;
            project_version = CLogger.Instance.ProjectVersion;
            appkey = CLogger.Instance.m_appKey;
        }

    }

    public class LogInfo
    {
        public string time { get; set; } // utc错误时间
        public string level { get; set; }
        public string log_string { get; set; }
        public string map_name { get; set; }

    }

    public class BaseData
    {
        public string session_id { get; set; }  // 每次启动生成
        public string unite_device_id { get; set; }  // 数据中心用户统一标识 外部传入
        public string uuid { get; set; }    //机器为一标识（首次安装生成保存本地）
        public string appkey { get; set; }  // 项目ID，由质量平台生成 初始化接口传入
        public string project_version { get; set; } //版本号 初始化接口传入
        public string channe_id { get; set; }   //渠道id 初始化接口传入
        public string branch_tag { get; set; }   // 主干/分支   trunk 初始化接口传入
        public string player_account { get; set; }   // 账号 初始化接口传入
        public string player_ip { get; set; }    // IP地址（多用于查询，如果是多层还会有一个出口IP）
        public string project_name { get; set; } // 项目名称
        public string time { get; set; }    // utc当前时间
        public string device_name { get; set; }  // 机器名称
        public string device_model { get; set; } // 机器型号（用得比较多）
        public string platform { get; set; }
        public string cpu_count { get; set; }
        public string cpu_frequency { get; set; }
        public string cpu_type { get; set; }
        public string memory { get; set; }
        public string os { get; set; }
        public string screen { get; set; }
        public string graphics_device_model { get; set; }
        public string engine_version { get; set; }
        public string sdk_version { get; set; }

        public BaseData()
        {
            session_id = CLogger.Instance.m_sessionId;
            uuid = CLogger.Instance.m_uuid;
            sdk_version = CLogger.SDK_VERSION;

            appkey = CLogger.Instance.m_appKey;
            project_version = CLogger.Instance.ProjectVersion;
            channe_id = CLogger.Instance.ChanneId;
            branch_tag = CLogger.Instance.BranchTag;
            player_account = CLogger.Instance.PlayerAccount;

            player_ip = CLogger.Instance.IpAddress;
            project_name = "";
            platform = "";

            // 设备信息
            time = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'");    //2020-12-23T09:55:45.864Z
            device_name = "";  //DESKTOP-7OA7K1F
            device_model = ""; //OptiPlex 9020 (Dell Inc.)
            cpu_count = "";    //4
            cpu_frequency = "";    //3193
            cpu_type = ""; //Intel(R) Core(TM) i5-4570 CPU @ 3.20GHz
            memory = "";  //8100
            os = "";  //Windows 10  (10.0.0) 64bit
            screen = "";  //595x279
            graphics_device_model = "";  //NVIDIA GeForce GTX 650 Ti
            engine_version = "";
        }

    }

    [Serializable]
    public class UserSave
    {
        public string uuid;
    }

    public class ResponseData
    {
        public string msg { get; set; }
        public int ret { get; set; }
        public string enable { get; set; }
        public string log_level { get; set; }
    }

}
