using Meebey.SmartIrc4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomeLib.Irc;
using Tomestone.Models;

namespace Tomestone.Chatting
{
    public partial class TomeChat
    {
        public void AdminCommands(Channel channel, IrcUser from, string message, string command)
        {
            if (channel.Name != Main.chatMods) return;

            switch (command)
            {
                case "@info":
                    _parse.ParseInfo(channel, message);
                    break;
                case "@delete":
                    _parse.ParseDelete(channel, message);
                    break;
                case "@edit":
                    _parse.ParseEdit(channel, message);
                    break;
                case "@entry":
                    _parse.ParseEntry(channel, message);
                    break;
                case "@add":
                    _parse.ParseSpecialAdd(channel, from, message);
                    break;
                case "@repeat":
                    _parse.ParseRepeat(channel, from, message);
                    break;
            }
        }

        public void UserCommands(Channel channel, IrcUser from, string message, string command)
        {
            if (channel.Name != Main.chatMain) return;

            switch (command)
            {
                case "!teach":
                    _parse.ParseTeach(channel, from, message);
                    break;
                case "!quote":
                    _parse.ParseQuote(channel, from, message);
                    break;
                case "!superquote":
                    _parse.ParseSuperQuote(channel, from, message);
                    break;
                case "!help":
                    _parse.ParseHelp(channel, message);
                    break;
                case "!get":
                    _parse.ParseGet(channel, message);
                    break;
                case "!add":
                    _parse.ParseAdd(channel, from, message);
                    break;
                case "!highlight":
                    _parse.ParseHighlight(channel, from, message);
                    break;
                default:
                    _parse.ParseSpecial(channel, from, message);
                    break;
            }
        }

        public void DefaultCommands(Channel channel, IrcUser from, string message, string command)
        {
            _parse.Reply(channel, from, message);
            ReceivedMessages.Add(new ChatMessage(from, message));
        }
    }
}
