using TomeLib.Irc;

namespace Tomestone.Chatting
{
    public class UserMessage : IMessage
    {
        public TwitchIrcChannel Channel { get; private set; }
        public TwitchChannelUser From { get; private set; }
        public string Message { get; set; }

        public UserMessage(TwitchIrcChannel channel, TwitchChannelUser from, string message)
        {
            Channel = channel;
            From = from;
            Message = message;
        }
    }
}