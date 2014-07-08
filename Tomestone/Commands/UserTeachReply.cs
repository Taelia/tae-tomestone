using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class UserTeachReply : ICommand
    {
        private readonly Dictionary<string, DateTime> _replies = new Dictionary<string, DateTime>();
        private readonly ChatDatabase _database;

        Random r = new Random();

        public UserTeachReply(ChatDatabase database)
        {
            _database = database;
        }

        public bool Parse(UserMessage message)
        {
            var random = r.Next(0, 100) < 5;

            return random;
        }

        public TomeReply Execute(UserMessage userMessage)
        {
            var search = userMessage.Message;

            var message = GetReplyFromDatabase(search);
            if (message == "") return null;

            message = ReplaceWildcards(userMessage.From.Nick, message);

            return new TomeReply(userMessage.Channel, message);
        }

        private string GetReplyFromDatabase(string search)
        {
            var table = _database.Tables["reply"];

            var entries = table.SearchBy("trigger", search);
            if (entries == null) 
                return "";

            var obj = PickUnusedReply(entries);
            if (obj == null) 
                return "";

            _database.ReplyCache.Add(obj);
            return obj.PrintMessage();
        }

        private string ReplaceWildcards(string user, string message)
        {
            //Replace wildcards with their corresponing replacements.
            var reply = message.Replace("%who", user);
            return reply;
        }

        private TableReply PickUnusedReply(List<TableReply> results)
        {
            //Where the reply is not in the list of replies, or if it is, where 10 minutes have passed since it's been put in there.
            var list = results.Where(x => !(_replies.ContainsKey(x.Message)) || DateTime.Now > _replies[x.Message] + TimeSpan.FromMinutes(10)).ToArray();
            if (list.Length == 0) return null;

            int random = r.Next(0, list.Length);

            var obj = list[random];

            if (!_replies.ContainsKey(obj.Message))
                _replies.Add(obj.Message, DateTime.Now);
            else
                _replies[obj.Message] = DateTime.Now;

            return obj;
        }
    }
}