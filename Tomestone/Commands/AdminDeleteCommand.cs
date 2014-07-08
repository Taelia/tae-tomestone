using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminDeleteCommand : BaseAdminCommand
    {
        private readonly ChatDatabase _database;
        protected override string RegexString { get { return "@delete (.+?) (.+)"; } }

        public AdminDeleteCommand(ChatDatabase database, string adminChannel) : base(adminChannel)
        {
            _database = database;
        }

        public override TomeReply Execute(UserMessage userMessage)
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