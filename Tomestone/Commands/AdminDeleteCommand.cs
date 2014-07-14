using System.Collections.Generic;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class AdminDeleteCommand : ICommand
    {
        private readonly TomeChat _chat;
        private const string RegexString = "^@delete (.+?) (.+)";

        readonly Dictionary<string, Table> _tables = new Dictionary<string, Table>();

        public AdminDeleteCommand(TomeChat chat)
        {
            _chat = chat;

            _tables.Add("command", new Table(Database.GetDatabase("tomestone.db"), "commands", "id"));
            _tables.Add("reply", new Table(Database.GetDatabase("tomestone.db"), "replies", "id"));
            _tables.Add("quote", new Table(Database.GetDatabase("tomestone.db"), "quotes", "id"));
        }

        public bool Parse(UserMessage message)
        {
            Match match = Regex.Match(message.Message, RegexString);
            var isAdminChannel = message.Channel.Name == _chat.Channels["mods"];

            return match.Success && isAdminChannel;
        }

        public void Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string tableName = match.Groups[1].Value.ToLower();
            string id = match.Groups[2].Value;

            var message = DeleteEntryFromDatabase(tableName, id);
            _chat.SendMessage(userMessage.Channel.Name, "/me :: " + message);
        }

        private string DeleteEntryFromDatabase(string tableName, string id)
        {
            if (!_tables.ContainsKey(tableName))
                return "Type not found. Choose from: 'command', 'reply', or 'quote'.";

            var table = _tables[tableName];
            var ok = table.Delete(id);
            if (!ok) return DefaultReplies.Error();

            tableName = char.ToUpper(tableName[0]) + tableName.Substring(1);
            return tableName + " #" + id + " has been deleted.";
        }
    }
}