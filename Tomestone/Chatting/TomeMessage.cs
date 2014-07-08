using Meebey.SmartIrc4net;
using Tomestone.Databases;

namespace Tomestone.Chatting
{
    public class TomeReply
    {
        public Channel Channel { get; private set; }
        public string Message { get; set; }

        public TomeReply(Channel channel, string message)
        {
            Channel = channel;
            Message = message;
        }

        public static string Angry()
        {
            return "Don't you even think about it.";
        }

        public static string Confirmation()
        {
            return "Sure. I guess.";
        }

        public static string Error()
        {
            return "No.";
        }

        public static string TypeNotFoundError(ChatDatabase database)
        {
            string w = "Type not found. Available: ";
            foreach (var tableName in database.Tables.Keys)
                w += tableName + ", ";

            //Trim final ','
            return w.Substring(0, w.Length - 1);
        }
    }
}