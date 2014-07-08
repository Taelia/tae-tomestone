using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminEditCommand : BaseAdminCommand
    {
        private readonly ChatDatabase _database;

        protected override string RegexString { get { return "@edit (.+?) (.+?) (.+?)=(.+)"; } }

        public AdminEditCommand(ChatDatabase database, string adminChannel) : base(adminChannel)
        {
            _database = database;
        }

        public override TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string type = match.Groups[1].Value;
            string id = match.Groups[2].Value;
            string toReplace = match.Groups[3].Value;
            string replaceWith = match.Groups[4].Value;

            var message = EditEntry(type, id, toReplace, replaceWith);
            return new TomeReply(userMessage.Channel, message);
        }

        private string EditEntry(string type, string id, string toReplace, string replaceWith)
        {
            if (type == "quote") 
                return TomeReply.Error();

            if (!_database.Tables.ContainsKey(type)) 
                return TomeReply.TypeNotFoundError(_database);

            var table = _database.Tables[type];

            var ok = table.Edit(id, toReplace, replaceWith);
            if (!ok) 
                return TomeReply.Error();

            var entry = table.GetById(id);
            return "Edited: " + entry.PrintInfo();
        }
    }
}