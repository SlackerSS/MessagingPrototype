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
using Application = System.Windows.Forms.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace MessagingServer
{
    public class HttpServer
    {
        private int _portNumber;
        private Socket _serverSocket, _clientSocket;
        private byte[] buffer;

        private static ManualResetEvent resetEvent;
        public HttpServer(int portNumber)
        {
            resetEvent = new ManualResetEvent(false);
            _portNumber = portNumber;
            var serverThread = new Thread(new ThreadStart(StartServer));
            serverThread.Start();
        }

        public void StartServer()
        {
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
                        Console.WriteLine("Waiting");
                        resetEvent.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                } 
        }

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

        public void RecieveCallBack(IAsyncResult AR)
        {
            try
            {
                int recieved = _clientSocket.EndReceive(AR);

                if (recieved == 0)
                {
                    return;
                }

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

        public static void AppendToTextBox(string text, RichTextBox box)
        {
            var invoker = new MethodInvoker(() => box.AppendText(text));
            invoker.Invoke();
        }
    }
}
