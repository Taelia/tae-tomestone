using Tomestone.Chatting;

namespace Tomestone.Databases
{
    public class TableReply
    {
        private IReplyType ReplyType { get; set; }

        public string Type { get { return ReplyType.Type; }}
        public string Id { get; private set; }
        public string AddedBy { get; private set; }
        public string Trigger { get; private set; }
        public string Message { get; private set; }

        public TableReply(IReplyType type, TableEntry entry)
        {
            ReplyType = type;

            Id = entry.Id;
            AddedBy = entry.AddedBy;
            Trigger = entry.Trigger;
            Message = entry.Reply;
        }

        public string PrintMessage()
        {
            return ReplyType.PrintMessage(this);
        }

        public string PrintInfo()
        {
            return ReplyType.PrintInfo(this);
        }
    }
}