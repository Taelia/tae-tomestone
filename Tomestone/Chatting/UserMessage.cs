
using System.Collections.Generic;
using System.Data;
using Meebey.SmartIrc4net;
using TomeLib.Irc;

namespace Tomestone.Chatting
{
    public class UserMessage : IMessage
    {
        public Channel Channel { get; private set; }
        public IrcUser From { get; private set; }
        public string Message { get; set; }

        public UserMessage(Channel channel, IrcUser from, string message)
        {
            Channel = channel;
            From = from;
            Message = message;
        }
    }
}