using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class SuperQuoteCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private string _user;

        private const string RegexString = "$quote (.+)";

        public SuperQuoteCommand(ChatDatabase database, string user)
        {
            _database = database;
            _user = user;
        }

        public bool Parse(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);
            return match.Success;
        }

        public TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string message = match.Groups[1].ToString();

            string reply = AddQuoteToDatabase(userMessage.From.Nick, message);
            return new TomeReply(userMessage.Channel, reply);
        }

        private string AddQuoteToDatabase(string from, string message)
        {
            var ok = _database.Tables["quote"].Insert(from, _user, message);
            if (!ok) return TomeReply.Error();

            var entry = _database.Tables["quote"].GetLatestEntry();
            _database.ReplyCache.Add(entry);
            return TomeReply.Confirmation();
        }
    }
}