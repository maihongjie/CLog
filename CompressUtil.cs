using System;
using System.Text;

namespace CLog
{
    class CompressUtil
    {
        internal static byte[] Compress(string rawString)
        {
            byte[] rawData = Encoding.UTF8.GetBytes(rawString);
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.Compression.GZipStream compressedzipStream = new System.IO.Compression.GZipStream(ms, System.IO.Compression.CompressionMode.Compress, true);
            compressedzipStream.Write(rawData, 0, rawData.Length);
            compressedzipStream.Close();
            byte[] result = ms.ToArray();
            ms.Close();
            return result;
        }
    }
}
