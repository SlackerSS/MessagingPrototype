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
using ExtensionMethods;
using Socket = System.Net.Sockets.Socket;
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace MessagingServer
{
    public class HttpServer
    {
        public static int portNumber = 8888;

        private static IPAddress _ipAddress;

        private Repository Repo;

        private readonly int _portNumber;

        private static int ClientsConnected;

        private List <Thread> _serverThreads;

        private List<Tuple<Socket, int>> _serverSockets;

        private List<Socket> _clientSockets;

        private static ManualResetEvent resetEvent;

        public HttpServer(int portNumber)
        {
            try
            {
                resetEvent = new ManualResetEvent(true);
                _serverSockets = new List<Tuple<Socket, int>>();
                _clientSockets = new List<Socket>();
                Console.WriteLine("Starting server");
                Repo = new Repository();
                resetEvent = new ManualResetEvent(false);
                _portNumber = portNumber;
                _ipAddress = Dns.GetHostEntry(IPAddress.Loopback).AddressList[0].MapToIPv4();
                Console.WriteLine($"On IP {_ipAddress} on port {portNumber}");
                Thread t = new Thread(() => StartServer());
                t.Start();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        public void StartServer()
        {
            _serverThreads = new List<Thread>();
            //TODO: Allow server to handle multiple client requests at same time
            try
            {
                    while (true)
                    {
                        _serverSockets.Add(Tuple.Create(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), portNumber));
                        _serverSockets.LastOrDefault().Item1.Bind(new IPEndPoint(IPAddress.Any, _portNumber));
                        _serverSockets.LastOrDefault().Item1.Listen(100);
                        resetEvent.Reset();

                        Console.WriteLine("waiting for client connection");
                        _serverSockets.LastOrDefault().Item1.BeginAccept(new AsyncCallback(AcceptClientStream),
                        _serverSockets.LastOrDefault().Item1);

                        resetEvent.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error in start server", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                } 
        }

        /// <summary>
        /// Accept current client stream result
        /// </summary>
        /// <param name="AR">Result of client stream</param>
        public void AcceptClientStream(IAsyncResult AR)
        {
            Console.WriteLine("Accepted client connection");
            byte[] buffer;
            resetEvent.Set();
            try
            {
                var serverSocket = (Socket) AR.AsyncState;
                var clientSocket = serverSocket.EndAccept(AR);
                _clientSockets.Add(clientSocket);
                buffer = new byte[clientSocket.ReceiveBufferSize];
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(RecieveCallBack), Tuple.Create(clientSocket, serverSocket, buffer));
                clientSocket.BeginAccept(new AsyncCallback(AcceptClientStream),
                            serverSocket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in AcceptClientStream", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        } 

        /// <summary>
        /// Recieve callback data from client stream
        /// </summary>
        /// <param name="AR">Async Result for client data interception</param>
        public void RecieveCallBack(IAsyncResult AR)
        {
            try
            {
                var tuple = (Tuple<Socket, Socket, byte[]>)AR.AsyncState;
                var clientSocket = tuple.Item1;
                var serverSocket = tuple.Item2;
                var buffer = tuple.Item3;
                var recieved = clientSocket.EndReceive(AR);

                if (recieved == 0)
                {
                    return;
                }

                MessageHandler handler = null;

                Array.Resize(ref buffer, recieved);
                string message = Encoding.ASCII.GetString(buffer, 0, buffer.Length);
                handler = message.FromJson<MessageHandler>();
                Console.WriteLine($"text: {handler.message} from: {handler.fromID} to: {handler.toID}");
                SendToClient(handler, serverSocket);
                Array.Resize(ref buffer, clientSocket.ReceiveBufferSize);
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                    new AsyncCallback(RecieveCallBack), Tuple.Create(clientSocket, serverSocket, buffer));
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
        public void SendToClient(MessageHandler message, Socket serverSocket)
        {
            var buffer = Encoding.ASCII.GetBytes(message.message);
            //TODO: Check if client socket is connected if not stall message
            serverSocket.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 1025),
                new AsyncCallback(SendToClientCallBack), serverSocket);
        }

        /// <summary>
        /// End stream after sending to client
        /// </summary>
        /// <param name="AR">Result from stream</param>
        public void SendToClientCallBack(IAsyncResult AR)
        {
            var serverSocket = (Socket)AR.AsyncState;
            var result = serverSocket.EndSend(AR);
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
