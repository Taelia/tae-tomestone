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
    public class UserHighlightCommand : ICommand
    {
        private readonly TomeChat _chat;

        private const string RegexString = "!highlight (.+)";

        public UserHighlightCommand(TomeChat chat)
        {
            _chat = chat;
        }

        public bool Parse(string message)
        {
            Match match = Regex.Match(message, RegexString);
            return match.Success;
        }

        public string Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            if (match.Success)
            {
                string message = match.Groups[1].ToString();

                HighlightIn30Minutes(userMessage.From.Nick, DateTime.Now.ToString("t"), message);
            }

            return "";
        }

        private async void HighlightIn30Minutes(string nick, string time, string message)
        {
            await Task.Delay(TimeSpan.FromMinutes(30));

            _chat.SendMessage(_chat.ModChannel.Name, nick + "suggested a highlight at " + time + ": " + message );
        }
    }
}
