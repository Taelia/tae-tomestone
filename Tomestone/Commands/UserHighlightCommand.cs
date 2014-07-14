using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class UserHighlightCommand : ICommand
    {
        private readonly TomeChat _chat;

        protected const string RegexString = "^!highlight (.+)";

        public UserHighlightCommand(TomeChat chat)
        {
            _chat = chat;
        }

        public bool Parse(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);
            return match.Success;
        }

        public void Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string description = match.Groups[1].ToString();

            HighlightIn30Minutes(userMessage.From.Nick, DateTime.Now.ToString("t"), description);
            _chat.SendMessage(userMessage.Channel.Name, DefaultReplies.Confirmation());
        }

        private async void HighlightIn30Minutes(string nick, string time, string description)
        {
            await Task.Delay(TimeSpan.FromMinutes(30));

            var message = nick + " suggested a highlight at " + time + ": " + description;
            _chat.SendMessage(_chat.Channels["mods"], "/me :: " + message);
        }
    }
}
