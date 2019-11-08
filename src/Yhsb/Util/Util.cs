using System.IO;

namespace Yhsb.Util
{
    public static class StreamExtension
    {
        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }
}