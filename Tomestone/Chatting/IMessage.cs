using Meebey.SmartIrc4net;

namespace Tomestone.Chatting
{
    public interface IMessage
    {
        Channel Channel { get; }
        IrcUser From { get; }
        string Message { get; }
    }
}