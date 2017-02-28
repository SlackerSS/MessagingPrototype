using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MessagingPrototype
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string VersionString = "0.1";
        private Version _versionNumber;

        public static RichTextBox MessagingBox;
        public MainWindow()
        {
            _versionNumber = Version.Parse(VersionString);
            InitializeComponent();
            MessengerWindow.Title = string.Format("Prototype Messenger v{0}", _versionNumber);
            MessagingBox = messagingBox;
        }

        public static void AppendToTextBox(string msg)
        {
            MessagingBox.AppendText(msg + "\r\n");
        }
    }
}
