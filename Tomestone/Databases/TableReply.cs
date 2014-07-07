using Tomestone.Chatting;

namespace Tomestone.Databases
{
    public class TableReply
    {
        private IReplyType Type { get; set; }

        public string Id { get; private set; }
        public string AddedBy { get; private set; }
        public string Trigger { get; private set; }
        public string Message { get; private set; }

        public TableReply(IReplyType type, TableEntry entry)
        {
            Type = type;

            Id = entry.Id;
            AddedBy = entry.AddedBy;
            Trigger = entry.Trigger;
            Message = entry.Reply;
        }

        public string PrintMessage()
        {
            return Type.PrintMessage(this);
        }

        public string PrintInfo()
        {
            return Type.PrintInfo(this);
        }
    }
}