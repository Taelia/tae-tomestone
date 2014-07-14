using System.Collections.Generic;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class AdminEntryCommand : ICommand
    {
        private TomeChat _chat;

        private const string RegexString = "^@entry (.+?) (.+)";

        Dictionary<string, Table> Tables = new Dictionary<string, Table>();

        public AdminEntryCommand(TomeChat chat)
        {
            _chat = chat;

            Tables.Add("command", new Table(Database.GetDatabase("tomestone.db"), "commands", "id"));
            Tables.Add("reply", new Table(Database.GetDatabase("tomestone.db"), "replies", "id"));
            Tables.Add("quote", new Table(Database.GetDatabase("tomestone.db"), "quotes", "id"));
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

            string table = match.Groups[1].ToString();
            string id = match.Groups[2].ToString();

            var message = GetTableEntry(table, id);
            _chat.SendMessage(userMessage.Channel.Name, "/me :: " + message);
        }

        private string GetTableEntry(string tableName, string id)
        {
            if (!Tables.ContainsKey(tableName)) return "Type not found. Choose from: 'command', 'reply', or 'quote'.";

            var table = Tables[tableName];
            var entry = table.GetById(id);
            if (entry == null) return DefaultReplies.Error();

            tableName = char.ToUpper(tableName[0]) + tableName.Substring(1);
            var message = "That was " + tableName + " #" + id + "( ";
            foreach (var column in entry.Columns)
                message += column.Key + " : " + column.Value + ", ";
            message = message.Substring(0, message.Length - 2);
            message += " )";

            return message;
        }
    }
}