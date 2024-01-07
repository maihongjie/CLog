using System;
using System.IO;
using System.Net;
using System.Threading;

namespace CLog
{

    public class UploadUtil
    {
        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="请求路径"></param>
        /// <param name="数据内容"></param>
        /// <param name="是否进行压缩"></param>
        /// <returns></returns>
        public static string Post(string url, string content, bool isCompress)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.Error("[CLog] Request address is Empty ");
                return null;
            }
            else if (string.IsNullOrEmpty(content))
            {
                Debug.Error("[CLog] Request context is Empty ");
                return null;
            }

            string res = null;

            for (var i = 0; i < 4; i++)
            {
                HttpWebRequest request = null;
                WebResponse response = null;
                try
                {
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = 30000;
                    request.Method = "POST";

                    byte[] bytes = null;
                    if (isCompress) // 字符串压缩
                    {
                        request.ContentType = "application/gzip";
                        bytes = CompressUtil.Compress(content);
                    }
                    else
                    {
                        bytes = System.Text.Encoding.UTF8.GetBytes(content);
                        request.ContentType = "application/json";
                    }
                    using (Stream writer = request.GetRequestStream())
                    {
                        writer.Write(bytes, 0, bytes.Length);
                    }

                    response = request.GetResponse();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        res = reader.ReadToEnd();
                    }
                    break;
                }
                catch (Exception ex)
                {
                    // 请求失败
                    if (i < 3)
                    {
                        Debug.Error(string.Format("[CLog] Request fail : <{0}>, Retry Time : {1} ", ex.Message, i + 1));
                        Thread.Sleep(1000);
                    }
                    else
                        Debug.Error("[CLog] Cannot connect server, Please check network ");
                }
                finally
                {
                    if (request != null)
                        request.Abort();
                    if (response != null)
                    {
                        response.Close();
                        response.Dispose();
                    }
                }
            }

            return res;

        }

    }

}
