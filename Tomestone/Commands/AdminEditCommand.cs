using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminEditCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly string _adminChannel;
        private const string RegexString = "@edit (.+?) (.+?) (.+?)=(.+)"; 

        public AdminEditCommand(ChatDatabase database, string adminChannel)
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