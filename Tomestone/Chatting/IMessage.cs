using TomeLib.Irc;

namespace Tomestone.Chatting
{
    public interface IMessage
    {
        TwitchIrcChannel Channel { get; }
        TwitchChannelUser From { get; }
        string Message { get; }
    }
}