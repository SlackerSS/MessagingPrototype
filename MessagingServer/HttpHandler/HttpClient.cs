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
using System.Runtime.Serialization.Formatters.Binary;
using Application = System.Windows.Forms.Application;

namespace MessagingServer
{
    public class HttpClient
    {        
        /// <summary>
        /// Connector used to connect client to database
        /// </summary>
        private Connector _currentConnector = null;
        
        /// <summary>
        /// Current application user
        /// </summary>
        private ApplicationUser _currentUser;

        /// <summary>
        /// String builder used for concatenating messages
        /// </summary>
        private StringBuilder builder;

        //Private client thread, don't know if needed yet
        private Thread clientThread;

        /// <summary>
        /// public instance of client socket
        /// </summary>
        public Socket _clientSocket;

        //ManualResetEvent for Sending
        private static ManualResetEvent sendDone;
        //Manual reset event for connect done
        private static ManualResetEvent connectDone;
        //Manual reset event for recieve done
        private static ManualResetEvent recieveDone;

        //Sending address
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

        public void SendMessage(MessageHandler Message)
        {
            try
            {
                var buffer = Message.ToByteArray();
                _clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None,
                        new AsyncCallback(BeginSendCallBack), null);
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

        public MessageHandler GenerateMessageHandler(string text, short from, short to)
        {
            return new MessageHandler(from, to, text);
        }

    }

    /// <summary>
    /// Class used to send messages to recepent
    /// </summary>
    public class Sender : IDisposable
    {
        public short UserIdTo;
        public short UserIdFrom;
        //TODO: Convert to type object for multiple message types
        public string message;
        public Sender(short UserIdTo, short UserIdFrom, string Message)
        {
            this.UserIdTo = UserIdTo;
            this.UserIdFrom = UserIdFrom;
            message = Message;
        }

        public void Dispose()
        {
            UserIdTo = 0;
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
