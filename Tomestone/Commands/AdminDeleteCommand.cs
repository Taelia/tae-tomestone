using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminDeleteCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly string _adminChannel;
        private const string RegexString = "@delete (.+?) (.+)";

        public AdminDeleteCommand(ChatDatabase database, string adminChannel)
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

            string tableName = match.Groups[1].Value;
            string id = match.Groups[2].Value;

            var ok = DeleteEntryFromDatabase(tableName, id);
            return new TomeReply(userMessage.Channel, ok);
        }

        private string DeleteEntryFromDatabase(string tableName, string id)
        {
            if (!_database.Tables.ContainsKey(tableName))
                return TomeReply.TypeNotFoundError(_database);

            var table = _database.Tables[tableName];
            var entry = table.GetById(id);

            var ok = table.Delete(id);
            if (!ok) 
                return TomeReply.Error();

            return entry.Type + " #" + id + " has been deleted.";
        }
    }
}