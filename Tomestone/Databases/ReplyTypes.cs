using System.Security.Cryptography.X509Certificates;
using System.Windows.Input;
using Tomestone.Databases;

namespace Tomestone.Chatting
{
    public interface IReplyType
    {
        string Type { get; }

        string PrintMessage(TableReply message);
        string PrintInfo(TableReply message);
    }

    public class DefaultReply : IReplyType
    {
        public string Type { get; private set; }

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
        public string Type { get; private set; }

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