using System;
using System.Collections.Generic;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class UserQuoteReply : ICommand
    {
        private readonly Dictionary<string, DateTime> _seenInLastHour = new Dictionary<string, DateTime>();

        Random r = new Random();

        private TomeChat _chat;

        Dictionary<string, Table> Tables = new Dictionary<string, Table>(); 

        public UserQuoteReply(TomeChat chat)
        {
            _chat = chat;

            Tables.Add("quote", new Table(Database.GetDatabase("tomestone.db"), "quotes", "id", "addedBy", "trigger", "reply"));
        }

        public bool Parse(UserMessage message)
        {
            var user = message.From.Nick;

            //If user hasn't been seen for an over hour, remove from the list.
            if (_seenInLastHour.ContainsKey(user) && _seenInLastHour[user] < DateTime.Now - TimeSpan.FromHours(1))
                _seenInLastHour.Remove(message.From.Nick);

            bool seen = _seenInLastHour.ContainsKey(user);
            bool random = r.Next(0, 100) < 10; 
            
            _seenInLastHour[user] = DateTime.Now;

            return random && !seen;
        }

        public void Execute(UserMessage userMessage)
        {
            var search = userMessage.From.Nick;

            var reply = GetQuoteFromDatabase(search);
            if (reply == null) return;

            _chat.SendMessage(userMessage.Channel.Name, reply);
        }

        private string GetQuoteFromDatabase(string search)
        {
            var table = Tables["quote"];

            var entry = table.GetRandomBy("trigger", search);
            if (entry == null)
                return null;

            var reply = CreateReply(entry);
            _chat.ReplyCache.Add(reply);
            return reply.Message;
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