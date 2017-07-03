using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;
using Vse.Web.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Web.Script.Serialization;
using MessagingServer.Users.Base;

namespace MessagingServer
{
    [Serializable]
    public class MessageHandler
    {
        //TODO: Convert user ids tto ints in future
        public string fromID { get; set; }
        public string toID { get; set; }
        public string message { get; set; }

        //public ApplicationUser user { get; set; }

    }
}
