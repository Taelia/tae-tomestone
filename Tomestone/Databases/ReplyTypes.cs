using System.Windows.Input;
using Tomestone.Databases;

namespace Tomestone.Chatting
{
    public interface IReplyType
    {
        string PrintMessage(TableReply message);
        string PrintInfo(TableReply message);
    }

    public class DefaultReply : IReplyType
    {
        private string Type { get; set; }

        public DefaultReply(string type)
        {
            Type = type;
        }

        public string PrintMessage(TableReply message)
        {
            return message.Message;
        }

        public string PrintInfo(TableReply message)
        {
            return Type + " #" + message.Id + " ( " + message.Trigger + " -> " + message.Message + " )";
        }
    }

    public class QuoteReply : IReplyType
    {
        private string Type { get; set; }

        public QuoteReply()
        {
            Type = "Quote";
        }

        public string PrintMessage(TableReply message)
        {
            return "\"" + message.Message + "\" -" + message.Trigger;
        }

        public string PrintInfo(TableReply message)
        {
            return Type + " #" + message.Id + " ( " + message.Trigger + " -> " + message.Message + " )";
        }
    }
}