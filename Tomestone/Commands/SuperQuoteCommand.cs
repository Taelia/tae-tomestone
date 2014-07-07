using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class SuperQuoteCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private string _name;

        private const string RegexString = "$quote (.+)";

        public SuperQuoteCommand(ChatDatabase database, string name)
        {
            _database = database;
            _name = name;
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

                AddQuoteToDatabase(userMessage.From.Nick, message);
            }

            return "";
        }

        private void AddQuoteToDatabase(string from, string message)
        {
            _database.Tables["quote"].Insert(from, _name, message);
            
            var entry = _database.Tables["quote"].GetLatestEntry();
            _database.ReplyCache.Add(entry);
        } 
    }
}