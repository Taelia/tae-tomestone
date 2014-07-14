namespace Tomestone.Chatting
{
    public class TomeReply
    {
        public string Message { get; set; }
        public string InfoMessage { get; set; }

        public TomeReply(string message, string info)
        {
            Message = message;
            InfoMessage = info;
        }

        
    }
}