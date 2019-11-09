using System;
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

    public static class PrintExtension
    {
        public static void Print<T>(this T obj)
        {
            Console.WriteLine($"{obj}");
        }
    }
}