using System.Collections.Generic;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class AdminEditCommand : ICommand
    {
        private TomeChat _chat;

        private const string RegexString = "^@edit (.+?) (.+?) (.+?)=(.+)";

        public Dictionary<string, Table> Tables = new Dictionary<string, Table>();

        public AdminEditCommand(TomeChat chat)
        {
            _chat = chat;

            Tables.Add("command", new Table(Database.GetDatabase("tomestone.db"), "commands", "id", "reply"));
            Tables.Add("reply", new Table(Database.GetDatabase("tomestone.db"), "replies", "id", "reply"));
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

            string type = match.Groups[1].Value.ToLower();
            string id = match.Groups[2].Value;
            string toReplace = match.Groups[3].Value;
            string replaceWith = match.Groups[4].Value;

            var message = EditEntry(type, id, toReplace, replaceWith);
            _chat.SendMessage(userMessage.Channel.Name, "/me :: " + message);
        }

        private string EditEntry(string tableName, string id, string toReplace, string replaceWith)
        {
            if (!Tables.ContainsKey(tableName))
                return "/me :: Type not found. Choose from: 'command' or 'reply'.";

            var table = Tables[tableName];
            var entry = table.GetById(id);

            var message = entry.Columns["reply"];
            var newMessage = message.Replace(toReplace, replaceWith);
            
            // "reply" is the name of the column we want to edit.
            var ok = table.Update(id, "reply", newMessage);
            if (!ok) return DefaultReplies.Error();

            entry.Columns["reply"] = newMessage;
            var tomeReply = CreateReply(tableName, entry);
            _chat.ReplyCache.Add(tomeReply);

            return "Edited: " + tomeReply.InfoMessage;
        }

        private static TomeReply CreateReply(string tableName, TableEntry entry)
        {
            var message = entry.Columns["reply"];

            tableName = char.ToUpper(tableName[0]) + tableName.Substring(1);

            var info = tableName + " #" + entry.Columns["id"] + " ( " + entry.Columns["trigger"] + " -> " + entry.Columns["reply"] + " )";

            var tomeReply = new TomeReply(message, info);
            return tomeReply;
        }
    }
}