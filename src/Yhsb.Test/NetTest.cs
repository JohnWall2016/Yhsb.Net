using System;
using Yhsb.Net;

namespace Yhsb.Test
{
    public class NetTest
    {
        public static void TestHttpSocket()
        {
            using var socket = new HttpSocket("124.228.42.248", 80);
            Console.WriteLine(socket.GetHttp("/"));
        }
    }
}