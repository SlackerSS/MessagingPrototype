using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagingServer.Users.Base;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MessagingServer.Users
{
    class Repository
    {
        //Temporary serializer for dummy database
        public XmlSerializer serializer;

        public TextWriter writer;
        public XmlReader reader;

        public string xmlPath;

        public List<ApplicationUser> currentUsers;

        public void SaveChanges()
        {
            writer = new StreamWriter((Stream)File.Create(xmlPath));
            serializer.Serialize(writer, currentUsers);
        }

        public ApplicationUser Find(short id)
        {
            return currentUsers.Single(x =>
                x.UserID == id);
        }
    }
}
