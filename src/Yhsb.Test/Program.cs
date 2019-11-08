using System;
using Yhsb.Net;

namespace Yhsb.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var socket = new HttpSocket("124.228.42.248", 80))
            {
                Console.WriteLine(socket.GetHttp("/"));
            }
        }
    }
}
