using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MessagingServer.Users.Base;
using System.Runtime.Serialization.Formatters.Binary;
using ExtensionMethods;
using Application = System.Windows.Forms.Application;

namespace MessagingServer
{
    public class HttpClient
    {        
        
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

        /// <summary>
        /// Reference instance used for server socket
        /// </summary>
        public Socket _serverSocket;

        //ManualResetEvent for Sending
        private ManualResetEvent sendDone;
        //Manual reset event for connect done
        private ManualResetEvent connectDone;
        //Manual reset event for recieve done
        private ManualResetEvent recieveDone;

        //Sending address
        public string sendAddress;

        public HttpClient(int portNumber, ref Socket serverSocket)
        {
            _serverSocket = serverSocket;
            BeginConnection(portNumber);
            sendDone = new ManualResetEvent(true);
            connectDone = new ManualResetEvent(true);
            recieveDone = new ManualResetEvent(true);
        }

        /// <summary>
        /// Begin connection to HttpServer using PortNumber
        /// <para>
        /// Currently connected to loopback
        /// </para>
        /// </summary>
        /// <param name="port">Port Number to begin connection to server</param>
        public void BeginConnection(int port)
        {
                Connector _currentConnector;
                try
                {
                    using (_currentConnector = new Connector(IPAddress.Loopback.ToString(), 8888))
                    {
                        _clientSocket = new Socket(_currentConnector.familyType, _currentConnector.socketType,
                            _currentConnector.protocolType);
                        _clientSocket.BeginConnect(
                            new IPEndPoint(_currentConnector.serverIp, _currentConnector.portNumber),
                            new AsyncCallback(FinalizeClientConnection), _clientSocket);

                        //connectDone.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"There was an issue beginning the connection \n {ex.Message}", Application.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(string.Format("There was an issue finalizing client connection, Error Message \n {0})", 
                    ex.Message), Application.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SendMessage(MessageHandler message)
        {
            try
            {
                var json = message.ToJSON();
                var buffer = ASCIIEncoding.ASCII.GetBytes(json);
                _clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None,
                        new AsyncCallback(BeginSendCallBack), null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in SendMessage function, Error message {ex.Message}", Application.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BeginSendCallBack(IAsyncResult AR)
        {
            try
            {
                _clientSocket.EndSend(AR);
            }catch(Exception ex)
            {
                MessageBox.Show($"Somehow this issue occurs in BeginSendCallBack {ex.Message}");
            }
        }

        public MessageHandler GenerateMessageHandler(string text, string from, string to)
        {
            return new MessageHandler()
            {
                fromID = from,
                toID = to,
                message = text
            };
        }

    }

    //TODO: Add HTTP Headers for client connection
    /// <summary>
    /// Temporary connector to connect client to the server
    /// </summary>
    public class Connector : IDisposable
    {
        public int portNumber { get; private set; }
        public IPAddress serverIp { get; private set; }
        public IPAddress localIp { get; private set; }
        public AddressFamily familyType { get; private set; }
        public SocketType socketType { get; private set; }
        public ProtocolType protocolType { get; private set; }

        public Connector(string ipAddres, int portNumber)
        {
            this.portNumber = portNumber;
            this.serverIp = IPAddress.Parse(ipAddres);
            this.familyType = AddressFamily.InterNetwork;
            this.socketType = SocketType.Stream;
            this.protocolType = ProtocolType.Tcp;
        }

        public void Dispose()
        {
            serverIp = null;
            portNumber = 0;
            localIp = null;
        }
    }
}
