using System;
using System.Text;
using System.Text.Json;
using System.IO;

namespace CLog
{
    internal class ConfigManager
    {
        private ConfigManager() { }
        internal static readonly ConfigManager Instance = new ConfigManager();
        private string m_filepath = Path.Combine(CLogger.Instance.m_basePath, "Config.json");

        internal void Serializer()
        {
            string jsonString = JsonSerializer.Serialize(new Config());
            File.WriteAllText(m_filepath, jsonString);
        }

        internal Config Deserializer()
        {
            string jsonString = File.ReadAllText(m_filepath);
            return JsonSerializer.Deserialize<Config>(jsonString);
        }
    }

    internal class Config {
        internal Debug.Level internalLogLevel = Debug.Level.None;
        internal string uploadInfoAddress = CLogger.Instance.uploadInfoAddress;
        internal string uploadLogAddress = CLogger.Instance.uploadLogAddress;
        internal bool enableSave = CLogger.Instance.m_enableSave;
        internal bool enableUpload = CLogger.Instance.m_enableUpload;
        internal CLogger.Level fileLogLevel = CLogger.Instance.m_fileLogLevel;
        internal CLogger.Level uploadLogLevel = CLogger.Instance.m_uploadLogLevel;
    }
}
