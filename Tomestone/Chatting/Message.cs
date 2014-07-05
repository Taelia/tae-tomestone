
using System.Collections.Generic;
using System.Data;
using Meebey.SmartIrc4net;
using TomeLib.Irc;

namespace Tomestone.Chatting
{
    public class ChatMessage : IMessage
    {
        public IrcUser From { get; private set; }
        public string Message { get; set; }
        public Dictionary<string, string> Data = new Dictionary<string, string>(); 

        public ChatMessage(IrcUser from, string message, Dictionary<string, string> data)
        {
            From = from;
            Message = message;
            Data = data;
        }

        public ChatMessage(IrcUser from, string message, DataRow dataRow)
        {
            From = from;
            Message = message;

            Data = DataRowToDictionary(dataRow);
        }

        public ChatMessage(IrcUser from, string message)
        {
            From = from;
            Message = message;
        }

        public string Info()
        {
            string w = "";
            foreach (var data in Data)
                w += data.Key + " : " + data.Value + " | ";
            return w;
        }

        private Dictionary<string, string> DataRowToDictionary(DataRow result)
        {
            var data = new Dictionary<string, string>();
            for (int i = 0; i < result.ItemArray.Length; i++)
                data[result.Table.Columns[i].ToString()] = result.ItemArray[i].ToString();
            return data;
        }
    }
}