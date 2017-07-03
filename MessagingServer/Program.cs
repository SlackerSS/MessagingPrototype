using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessagingServer
{
    class Program
    {
        private static HttpServer _currentHttpServer;
        static void Main(string[] args)
        {
            InitalizeServer(8888);
        }

        private static void InitalizeServer(int portNumber)
        {
            _currentHttpServer = new HttpServer(portNumber);
        }
    }
}
