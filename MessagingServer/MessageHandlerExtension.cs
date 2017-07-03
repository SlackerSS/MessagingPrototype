using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagingServer;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace ExtensionMethods
{
    public static class MessageHandlerExtension
    {
        public static string ToJSON(this object obj)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        public static string ToJSON(this object obj, int recursionDepth)
        {
            var serializer = new JavaScriptSerializer();
            serializer.RecursionLimit = recursionDepth;
            return serializer.Serialize(obj);
        }

        public static T FromJson<T>(this string obj)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(obj);
        }
    }
}
