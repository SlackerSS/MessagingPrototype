using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessagingServer.Users.Base
{
    class ApplicationUser
    {
        public string UserName;
        public IPAddress IpAddress;

        public HttpClient currentClient;

        private short _id;

        public short UserID
        {
            get { return _id; }
            set { _id = value; }
        }

        public ApplicationUser(string userName, string ipAddress, short idNumber)
        {
            this.UserName = userName;
            this.IpAddress = IPAddress.Parse(ipAddress);
        }
    }
}
