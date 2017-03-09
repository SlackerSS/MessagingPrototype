using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MessagingServer.Users;
using MessagingServer.Users.Base;

using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace MessagingServer
{
    public class HttpServer
    {
        /// <summary>
        /// private instance of repository/database
        /// </summary>
        private Repository Repo;
        /// <summary>
        /// Port number that server will be instantiated on (ex 80)
        /// </summary>
        private readonly int _portNumber;

        /// <summary>
        /// private socket for server
        /// </summary>
        private Socket _serverSocket;
        /// <summary>
        /// private gobal instance for client socket
        /// </summary>
        private Socket _clientSocket;
        private byte[] buffer;

        /// <summary>
        /// Manual Reset Event for handeling events
        /// </summary>
        private static ManualResetEvent resetEvent;

        /// <summary>
        /// Initializes server information
        /// </summary>
        /// <param name="portNumber">Number for server port</param>
        public HttpServer(int portNumber)
        {
            Repo = new Repository();
            resetEvent = new ManualResetEvent(false);
            _portNumber = portNumber;
            var serverThread = new Thread(new ThreadStart(StartServer));
            serverThread.Start();
        }

        /// <summary>
        /// Begin listening for client requests
        /// </summary>
        public void StartServer()
        {
            //TODO: Allow server to handle multiple client requests at same time
            try
            {
                    _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _portNumber));
                    _serverSocket.Listen(100);
                    while (true)
                    {
                        resetEvent.Reset();

                        Console.WriteLine("waiting for client connection");
                        _serverSocket.BeginAccept(new AsyncCallback(AcceptClientStream), 
                            _serverSocket);

                        resetEvent.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                } 
        }

        /// <summary>
        /// Accept current client stream result
        /// </summary>
        /// <param name="AR">Result of client stream</param>
        public void AcceptClientStream(IAsyncResult AR)
        {
            Console.WriteLine("Accepted client connection");
            resetEvent.Set();
            try
            {
                _clientSocket = _serverSocket.EndAccept(AR);
                buffer = new byte[_clientSocket.ReceiveBufferSize];
                _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(RecieveCallBack), _clientSocket);
                _serverSocket.BeginAccept(new AsyncCallback(AcceptClientStream), 
                            _serverSocket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AcceptClientData()
        {
            //TODO: Accept all client data as MessagePackage and send to second client
        }

        /// <summary>
        /// Recieve callback data from client stream
        /// </summary>
        /// <param name="AR">Async Result for client data interception</param>
        public void RecieveCallBack(IAsyncResult AR)
        {
            try
            {
                int recieved = _clientSocket.EndReceive(AR);

                if (recieved == 0)
                {
                    return;
                }

                //MessageHandler handler = new MessageHandler();

                Array.Resize(ref buffer, recieved);
                string text = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                Console.WriteLine(text);
                Array.Resize(ref buffer, _clientSocket.ReceiveBufferSize);
                _clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(RecieveCallBack), _clientSocket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Send message as MessageHandler to desired client
        /// </summary>
        /// <param name="message"></param>
        public void SendToClient(MessageHandler message)
        {
            var buffer = message.ToByteArray();
            //TODO: Check if client socket is connected if not stall message
            _serverSocket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, new IPEndPoint(Repo.Find(6499).IpAddress, 3332),
                new AsyncCallback(SendToClientCallBack), null);
        }

        /// <summary>
        /// End stream after sending to client
        /// </summary>
        /// <param name="AR">Result from stream</param>
        public void SendToClientCallBack(IAsyncResult AR)
        {
            _serverSocket.EndSend(AR);
        }
        /// <summary>
        /// Used for debugging append text to client box
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <param name="box">Rich text box on client form</param>
        public static void AppendToTextBox(string text, RichTextBox box)
        {
            var invoker = new MethodInvoker(() => box.AppendText(text));
            invoker.Invoke();
        }
    }
}
