using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessagingServer
{
    class HttpClient
    {
        private Socket _clientSocket;

        private void BeginConnection()
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.BeginConnect(new IPEndPoint(IPAddress.Loopback, 3333),
                    new AsyncCallback(FinalizeClientConnection), null);
            }
        }

        private void FinalizeClientConnection(IAsyncResult AR)
        {
            _clientSocket.EndConnect(AR);

        }
    }
}
