using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminRepeatCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly string _adminChannel;
        private const string RegexString =  "@repeat (([01][0-9]|2[0-3]):[0-5][0-9][ap]m) (.+)";

        public AdminRepeatCommand(ChatDatabase database, string adminChannel)
        {
            _database = database;
            _adminChannel = adminChannel;
        }

        public bool Parse(UserMessage message)
        {
            Match match = Regex.Match(message.Message, RegexString);
            var isAdminChannel = message.Channel.Name == _adminChannel;

            return match.Success && isAdminChannel;
        }

        public TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string trigger = match.Groups[1].ToString();
            string reply = match.Groups[2].ToString();

            var message = AddRepeatToDatabase(userMessage.From.Nick, trigger, reply);
            return new TomeReply(userMessage.Channel, message);
        }

        private string AddRepeatToDatabase(string from, string trigger, string reply)
        {
            var ok = _database.Tables["repeat"].Insert(from, trigger.ToLower(), reply);
            if (!ok) return TomeReply.Error();

            var entry = _database.Tables["repeat"].GetLatestEntry();
            _database.ReplyCache.Add(entry);

            return TomeReply.Confirmation();
        }
    }
}