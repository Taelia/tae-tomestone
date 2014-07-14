using System.Collections.Generic;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class UserQuoteCommand : ICommand
    {
        private readonly TomeChat _chat;

        readonly Dictionary<string, Table> _tables = new Dictionary<string, Table>(); 

        private const string RegexString = "^!quote (.+?) (.+)";

        public UserQuoteCommand(TomeChat chat)
        {
            _chat = chat;

            _tables.Add("quote", new Table(Database.GetDatabase("tomestone.db"), "quotes", "id", "addedBy", "trigger", "reply"));
        }

        public bool Parse(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);
            return match.Success;
        }

        public void Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string user = match.Groups[1].ToString();
            string search = match.Groups[2].ToString();

            var reply = AddQuoteToDatabase(userMessage.From.Nick, user, search);
            _chat.SendMessage(userMessage.Channel.Name, reply);
        }

        private string AddQuoteToDatabase(string from, string user, string search)
        {
            //Get the latest message from user containing the search string.
            var userMessage = _chat.ReceivedMessages.Search(user, search);
            if (userMessage == null)
                return DefaultReplies.Error();

            var data = new Dictionary<string, string>();
            data["addedBy"] = from;
            data["trigger"] = user;
            data["reply"] = userMessage.Message;

            var ok = _tables["quote"].Insert(data);
            if (!ok) 
                return DefaultReplies.Error();

            var entry = _tables["quote"].GetLatestEntry();
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
