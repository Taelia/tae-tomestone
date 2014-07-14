using System.Linq;
using System.Text.RegularExpressions;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class AdminInfoCommand : ICommand
    {
        private readonly TomeChat _chat;

        private const string RegexString = "^@info (.+)"; 

        public AdminInfoCommand(TomeChat chat)
        {
            _chat = chat;
        }

        public bool Parse(UserMessage message)
        {
            Match match = Regex.Match(message.Message, RegexString);
            var isAdminChannel = message.Channel.Name == _chat.Channels["mods"];

            return match.Success && isAdminChannel;
        }

        public void Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string search = match.Groups[1].ToString();

            var message = GetInfoFromDatabase(search);

            _chat.SendMessage(userMessage.Channel.Name, "/me :: " + message);
        }

        private string GetInfoFromDatabase(string search)
        {
            //Find the latest message sent that contains 'search' within the message.
            var entry = _chat.ReplyCache.LastOrDefault(x => x.Message.Contains(search));

            if (entry == null)
                return "I did not send any messages containing '" + search + "'.";

            return entry.InfoMessage;
        }
    }
}
