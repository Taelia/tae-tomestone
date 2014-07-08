using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class UserQuoteReply : ICommand
    {
        private readonly Dictionary<string, DateTime> _replies = new Dictionary<string, DateTime>();
        private readonly ChatDatabase _database;

        Random r = new Random();

        public UserQuoteReply(ChatDatabase database)
        {
            _database = database;
        }

        public bool Parse(UserMessage message)
        {
            var random = r.Next(0, 100) < 25;

            return random;
        }

        public TomeReply Execute(UserMessage userMessage)
        {
            var search = userMessage.Message;

            var message = GetQuoteFromDatabase(search);
            if (message == "") return null;

            return new TomeReply(userMessage.Channel, message);
        }

        private string GetQuoteFromDatabase(string search)
        {
            var table = _database.Tables["quote"];

            var entry = table.GetRandomBy("trigger", search);
            if (entry == null)
                return "";

            _database.ReplyCache.Add(entry);
            return entry.PrintMessage();
        }
    }
}