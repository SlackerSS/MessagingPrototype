using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MessagingServer.Users.Base;
using Application = System.Windows.Forms.Application;

namespace MessagingServer
{
    public class HttpClient
    {
        private Sender _currentSender = null;
        private Connector _currentConnector = null;
        private ApplicationUser _currentUser;

        private Thread clientThread;

        public Socket _clientSocket;
        private readonly RichTextBox _box;

        private static ManualResetEvent sendDone;
        private static ManualResetEvent connectDone;
        private static ManualResetEvent recieveDone;

        public string sendAddress;

        public HttpClient(int portNumber)
        {
            BeginConnection(portNumber);
        }

        public void BeginConnection(int port)
        {
                try
                {
                    using (_currentConnector = new Connector(IPAddress.Loopback.ToString(), 3333))
                    {
                        _clientSocket = new Socket(_currentConnector.familyType, _currentConnector.socketType,
                            _currentConnector.protocolType);
                        _clientSocket.BeginConnect(
                            new IPEndPoint(_currentConnector.serverIp, _currentConnector.portNumber),
                            new AsyncCallback(FinalizeClientConnection), _clientSocket);

                        connectDone.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        private void FinalizeClientConnection(IAsyncResult AR)
        {
            try
            {
                _clientSocket.EndConnect(AR);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("There was an issue finalizing client connection, Error Message \n {0)", 
                    ex.Message), Application.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SendMessage(string text)
        {
            try
            {
                using (_currentSender = new Sender(string.Empty, string.Empty, text))
                {
                    var buffer = Encoding.ASCII.GetBytes(_currentSender.message);
                    _clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None,
                        new AsyncCallback(BeginSendCallBack), null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BeginSendCallBack(IAsyncResult AR)
        {
                _clientSocket.EndSend(AR);
        }
    }

    /// <summary>
    /// Class used to send messages to recepent
    /// </summary>
    public class Sender : IDisposable
    {
        public string UserNameTo;
        public string UserNameFrom;
        //TODO: Convert to type object for multiple message types
        public string message;
        public Sender(string IpAddressFrom, string IpAddressTo, string Message)
        {
            UserNameTo = IpAddressTo;
            UserNameFrom = IpAddressFrom;
            message = Message;
        }

        public void Dispose()
        {
            UserNameTo = null;
            message = string.Empty;
        }
    }

    //TODO: Add HTTP Headers for client connection
    /// <summary>
    /// Temporary connector to connect client to the server
    /// </summary>
    public class Connector : IDisposable
    {
        public int portNumber;
        public IPAddress serverIp;
        public AddressFamily familyType;
        public SocketType socketType;
        public ProtocolType protocolType;

        public Connector(string ipAddres, int portNumber)
        {
            this.portNumber = portNumber;
            this.serverIp = IPAddress.Parse(ipAddres);
            familyType = AddressFamily.InterNetwork;
            socketType = SocketType.Stream;
            protocolType = ProtocolType.Tcp;
        }

        public void Dispose()
        {
            serverIp = null;
        }
    }
}
