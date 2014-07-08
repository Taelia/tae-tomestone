using System;
using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminEntryCommand : BaseAdminCommand
    {
        private readonly ChatDatabase _database;

        protected override string RegexString { get { return "@entry (.+?) (.+)"; } }

        public AdminEntryCommand(ChatDatabase database, string adminChannel) : base(adminChannel)
        {
            _database = database;
        }

        public override TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string table = match.Groups[1].ToString();
            string id = match.Groups[2].ToString();

            var message = GetTableEntry(table, id);
            return new TomeReply(userMessage.Channel, message);
        }

        private string GetTableEntry(string tableName, string id)
        {
            if (!_database.Tables.ContainsKey(tableName))
                return TomeReply.TypeNotFoundError(_database);

            var table = _database.Tables[tableName];
            var entry = table.GetById(id);

            if (entry == null)
                return TomeReply.Error();

            return entry.PrintInfo();
        }
    }
}