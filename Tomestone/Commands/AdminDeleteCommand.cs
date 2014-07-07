using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminDeleteCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly TomeChat _chat;

        private const string RegexString = "@delete (.+?) (.+)";

        public AdminDeleteCommand(ChatDatabase database, TomeChat chat)
        {
            _database = database;
            _chat = chat;
        }

        public bool Parse(string message)
        {
            Match match = Regex.Match(message, RegexString);
            return match.Success;
        }

        public string Execute(UserMessage userMessage)
        {
            if (userMessage.Channel != _chat.ModChannel)
                return "";

            Match match = Regex.Match(userMessage.Message, RegexString);

            if (match.Success)
            {
                string type = match.Groups[1].Value;
                string id = match.Groups[2].Value;

                DeleteEntryFromDatabase(userMessage.From.Nick, type, id);
            }

            return "";
        }

        private void DeleteEntryFromDatabase(string from, string tableName, string id)
        {
            if (!_database.Tables.ContainsKey(tableName))
            {
                _chat.SendMessage(_chat.ModChannel.Name, TypeNotFoundError());
                return;
            }

            var table = _database.Tables[tableName];
            var ok = table.Delete(id);
        }

        private string TypeNotFoundError()
        {
            string w = "Type not found. Available: ";
            foreach (var tableName in _database.Tables.Keys)
                w += tableName + ", ";

            //Trim final ','
            return w.Substring(0, w.Length - 1);
        }
    }
}