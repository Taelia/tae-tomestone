using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class UserQuoteCommand : BaseUserCommand
    {
        private readonly ChatDatabase _database;
        private readonly History<UserMessage> _history;

        protected override string RegexString { get { return "!quote (.+?) (.+)"; } }

        public UserQuoteCommand(ChatDatabase database, History<UserMessage> history)
        {
            _database = database;
            _history = history;
        }

        public override TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string user = match.Groups[1].ToString();
            string search = match.Groups[2].ToString();

            var reply = AddQuoteToDatabase(userMessage.From.Nick, user, search);
            return new TomeReply(userMessage.Channel, reply);
        }

        private string AddQuoteToDatabase(string from, string user, string search)
        {
            //Get the latest message from user containing the search string.
            var userMessage = _history.Search(user, search);

            var ok = _database.Tables["quote"].Insert(from, userMessage.From.Nick, userMessage.Message);
            if (!ok) 
                return TomeReply.Error();

            var entry = _database.Tables["quote"].GetLatestEntry();
            _database.ReplyCache.Add(entry);

            return TomeReply.Confirmation();
        }
    }
}
