using TomeLib.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tomestone.Chatting
{
    public partial class TomeChat
    {
        public void SendStatus(string channel, string message)
        { Irc.SendMessage("/me :: " + message, channel); }
        public void SendMessage(string channel, string message)
        { Irc.SendMessage(message, channel); }
    }
}
