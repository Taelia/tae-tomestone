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
    public class UserQuoteCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly History<UserMessage> _history; 

        private const string RegexString = "!quote (.+?) (.+)";

        public UserQuoteCommand(ChatDatabase database, History<UserMessage> history )
        {
            _database = database;
            _history = history;
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
                string user = match.Groups[1].ToString();
                string search = match.Groups[2].ToString();

                AddQuoteToDatabase(userMessage.From.Nick, user, search);
            }

            return "";
        }

        private void AddQuoteToDatabase(string from, string user, string search)
        {
            //Get the latest message from user containing the search string.
            var userMessage = _history.Search(user, search);

            _database.Tables["quote"].Insert(from, userMessage.From.Nick, userMessage.Message);
            
            var entry = _database.Tables["quote"].GetLatestEntry();
            _database.ReplyCache.Add(entry);
        }
    }
}
