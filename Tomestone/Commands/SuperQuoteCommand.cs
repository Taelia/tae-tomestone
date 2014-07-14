using System.Collections.Generic;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class SuperQuoteCommand : ICommand
    {
        private TomeChat _chat;
        private string _user = "taelia_";
        private const string RegexString = @"^\$quote (.+)";

        Dictionary<string, Table> Tables = new Dictionary<string, Table>(); 

        public SuperQuoteCommand(TomeChat chat)
        {
            _chat = chat;

            Tables.Add("quote", new Table(Database.GetDatabase("tomestone.db"), "quotes", "id", "addedBy", "trigger", "reply"));
        }

        public bool Parse(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);
            return match.Success;
        }

        public void Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string message = match.Groups[1].ToString();

            string reply = AddQuoteToDatabase(userMessage.From.Nick, message);
            _chat.SendMessage(userMessage.Channel.Name, reply);
        }

        private string AddQuoteToDatabase(string from, string message)
        {
            var data = new Dictionary<string, string>();
            data["addedBy"] = from;
            data["trigger"] = _user;
            data["reply"] = message;

            var ok = Tables["quote"].Insert(data);
            if (!ok) return DefaultReplies.Error();

            var entry = Tables["quote"].GetLatestEntry();
            var reply = CreateReply(entry);
            _chat.ReplyCache.Add(reply);
            return DefaultReplies.Confirmation();
        }

        private static TomeReply CreateReply(TableEntry entry)
        {
            var message = "\"" + entry.Columns["reply"] + "\" -" + entry.Columns["trigger"];

            var info = "Quote #" + entry.Columns["id"] + " ( " + entry.Columns["trigger"] + " -> " + entry.Columns["reply"] + " )";

            var tomeReply = new TomeReply(message, info);
            return tomeReply;
        }
    }
}