using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MessagingServer
{
    [Serializable]
    public class MessageHandler : ISerializable
    {
        //TODO: Convert user ids tto ints in future
        public short fromID;
        public short toID;
        public string message;

        public Sender _currentSender;

        BinaryFormatter Formatter;

        public MessageHandler(short fromID, short toID, string Message)
        {
            this.fromID = fromID;
            this.toID = toID;
            this.message = Message;
        }
        


        //TODO: Implement better serialization method for safe data transfoer
        public void SerializeMessage(ref NetworkStream messageStream)
        {
            try
            {
                Formatter.Serialize(messageStream, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Messaging Server Prototype", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public Sender SerializeMessageToSender(ref NetworkStream messageStream)
        {
            try
            {
                Formatter.Serialize(messageStream, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Messaging prototype", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return new Sender(fromID, toID, message);
        }

        /// <summary>
        /// Called during object serialization, neccessary for ISerializable 
        /// </summary>
        /// <param name="info">Serialization info, must be populated</param>
        /// <param name="context">Streaming context</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("from", fromID, typeof(short));
            info.AddValue("to", toID, typeof(short));
            info.AddValue("message", message, typeof(string));  
        }

        public MessageHandler(byte[] data)
        {
            fromID = BitConverter.ToInt16(data, 0);
            toID = BitConverter.ToInt16(data, 2);
            int messageLength = BitConverter.ToInt32(data, 3);
            message = Encoding.ASCII.GetString(data, 7, messageLength);
        }

        public byte[] ToByteArray()
        {
            List<byte> byteList = new List<byte>();
            byteList.AddRange(BitConverter.GetBytes(fromID));
            byteList.AddRange(BitConverter.GetBytes(toID));
            byteList.AddRange(BitConverter.GetBytes(message.Length));
            byteList.AddRange(Encoding.ASCII.GetBytes(message));

            return byteList.ToArray();
        }
    }
}
