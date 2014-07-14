using System;
using System.Collections.Generic;
using System.Linq;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class UserTeachReply : ICommand
    {
        private TomeChat _chat;

        private readonly Dictionary<string, DateTime> _replies = new Dictionary<string, DateTime>();

        Random r = new Random();

        Dictionary<string, Table> Tables = new Dictionary<string, Table>(); 

        public UserTeachReply(TomeChat chat)
        {
            _chat = chat;

            Tables.Add("reply", new Table(Database.GetDatabase("tomestone.db"), "replies", "id", "addedBy", "trigger", "reply"));
        }

        public bool Parse(UserMessage message)
        {
            var random = r.Next(0, 100) < 5;

            return random;
        }

        public void Execute(UserMessage userMessage)
        {
            var search = userMessage.Message;

            var reply = GetReplyFromDatabase(search);
            if (reply == "") return;

            reply = ReplaceWildcards(userMessage.From.Nick, reply);

            _chat.SendMessage(userMessage.Channel.Name, reply);
        }

        private string GetReplyFromDatabase(string search)
        {
            var table = Tables["reply"];

            var entries = table.SearchBy("trigger", search);
            if (entries == null) 
                return "";

            var entry = PickUnusedReply(entries);
            if (entry == null) 
                return "";

            var reply = CreateReply(entry);
            _chat.ReplyCache.Add(reply);
            return entry.Columns["reply"];
        }

        private static TomeReply CreateReply(TableEntry entry)
        {
            var message = entry.Columns["reply"];

            var info = "Reply #" + entry.Columns["id"] + " ( " + entry.Columns["trigger"] + " -> " + entry.Columns["reply"] + " )";

            var tomeReply = new TomeReply(message, info);
            return tomeReply;
        }

        private string ReplaceWildcards(string user, string message)
        {
            //Replace wildcards with their corresponing replacements.
            var reply = message.Replace("%who", user);
            return reply;
        }

        private TableEntry PickUnusedReply(List<TableEntry> results)
        {
            //Where the reply is not in the list of replies, or if it is, where 10 minutes have passed since it's been put in there.
            var list = results.Where(x => !(_replies.ContainsKey(x.Columns["reply"])) || DateTime.Now > _replies[x.Columns["reply"]] + TimeSpan.FromMinutes(10)).ToArray();
            if (list.Length == 0) return null;

            int random = r.Next(0, list.Length);

            var obj = list[random];

            if (!_replies.ContainsKey(obj.Columns["reply"]))
                _replies.Add(obj.Columns["reply"], DateTime.Now);
            else
                _replies[obj.Columns["reply"]] = DateTime.Now;

            return obj;
        }
    }
}