using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class SuperTeachReply : ICommand
    {
        private readonly TomeChat _chat;

        private readonly Dictionary<string, DateTime> _replies = new Dictionary<string, DateTime>();

        readonly Dictionary<string, Table> _tables = new Dictionary<string, Table>(); 

        protected string RegexString { get { return "^Tome, (.+)|(.+), Tome.?$"; } }

        readonly Random _random = new Random();

        public SuperTeachReply(TomeChat chat)
        {
            _chat = chat;

            _tables.Add("reply", new Table(Database.GetDatabase("tomestone.db"), "replies", "id", "addedBy", "trigger", "reply"));
        }

        public bool Parse(UserMessage message)
        {
            Match match = Regex.Match(message.Message, RegexString);

            return match.Success;
        }

        public void Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);


            var search = match.Groups[1].ToString();
            if (search == "") search = match.Groups[2].ToString();

            var reply = GetReplyFromDatabase(search);
            if (reply == null) return;

            reply = ReplaceWildcards(userMessage.From.Nick, reply);

            _chat.SendMessage(userMessage.Channel.Name, reply);
        }

        private string GetReplyFromDatabase(string search)
        {
            var table = _tables["reply"];

            var entries = table.SearchBy("trigger", search);
            if (entries == null) 
                return null;

            var entry = PickUnusedReply(entries);
            if (entry == null) 
                return null;

            var reply = CreateReply(entry);
            _chat.ReplyCache.Add(reply);

            return reply.Message;
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

            int r = _random.Next(0, list.Length);

            var obj = list[r];

            if (!_replies.ContainsKey(obj.Columns["reply"]))
                _replies.Add(obj.Columns["reply"], DateTime.Now);
            else
                _replies[obj.Columns["reply"]] = DateTime.Now;

            return obj;
        }
    }
}