using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Net.Sockets;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessagingServer;

namespace MessagingPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string VersionString = "0.1";

        private HttpClient client;

        public static RichTextBox MessagingBox;
        public MainWindow()
        {
            var _versionNumber = Version.Parse(VersionString);
            InitializeComponent();
            MessengerWindow.Title = string.Format("Prototype Messenger v{0}", _versionNumber);
            InitalizeClientConnection();
        }

        public static void AppendToTextBox(string msg)
        {
            HttpServer.AppendToTextBox(msg, MessagingBox);
        }

        private void InitalizeClientConnection()
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client = new HttpClient(80, ref socket);
        }


        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var text = string.Empty;
            if ((text = new TextRange(messagingBox.Document.ContentStart, messagingBox.Document.ContentEnd).Text).Length > 0 && !string.IsNullOrWhiteSpace(text))
            {
                client.SendMessage(client.GenerateMessageHandler(text, "127.0.0.1", "127.0.0.1"));
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (client._clientSocket != null && client._clientSocket.Connected )
            {
                client._clientSocket.Close();
            }
        }
    }
}
