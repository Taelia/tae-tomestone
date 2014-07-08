using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class UserHighlightCommand : BaseUserCommand
    {
        private readonly TomeChat _chat;

        protected override string RegexString { get { return "!highlight (.+)"; } }

        public UserHighlightCommand(TomeChat chat)
        {
            _chat = chat;
        }

        public override TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string description = match.Groups[1].ToString();

            HighlightIn30Minutes(userMessage.From.Nick, DateTime.Now.ToString("t"), description);
            return new TomeReply(userMessage.Channel, TomeReply.Confirmation());
        }

        private async void HighlightIn30Minutes(string nick, string time, string message)
        {
            await Task.Delay(TimeSpan.FromMinutes(30));

            _chat.SendMessage(_chat.Channels["mods"], nick + "suggested a highlight at " + time + ": " + message);
        }
    }
}
