using TomeLib.Db;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TomeLib.Irc;
using Tomestone.Chatting;
using Meebey.SmartIrc4net;
using Tomestone.Databases;
using Tomestone.Models;

namespace Tomestone.Commands
{
    public class DefaultCommands
    {
        private Dictionary<string, DateTime> _replies = new Dictionary<string, DateTime>();

        private TomeChat _chat;
        private ChatDatabase _database;

        public DefaultCommands(TomeChat chat, ChatDatabase database)
        {
            _chat = chat;
            _database = database;
        }

        private TableReply PickRandomReply(List<TableReply> results)
        {
            //Where the reply is not in the list of replies, or if it is, where 10 minutes have passed since it's been put in there.
            var list = results.Where(x => !(_replies.ContainsKey(x.Message)) || DateTime.Now > _replies[x.Message] + TimeSpan.FromMinutes(10)).ToArray();
            if (list.Length == 0) return null;

            var random = new Random();
            int r = random.Next(0, list.Length);

            var obj = list[r];

            if (!_replies.ContainsKey(obj.Message))
                _replies.Add(obj.Message, DateTime.Now);
            else
                _replies[obj.Message] = DateTime.Now;

            return obj;
        }

        public void Reply(Channel channel, IrcUser from, string message)
        {
            var search = message;

            var table = _database.Tables["reply"];

            var entries = table.SearchBy("trigger", search);
            if (entries == null) return;

            var obj = PickRandomReply(entries);
            if (obj == null) return;

            _database.ReplyCache.Add(obj);

            //Replace wildcards with their corresponing replacements.
            var reply = obj.Message.Replace("%who", from.Nick);

            //Finally, send the message.
            _chat.SendMessage(channel.Name, reply);
        }

        public void Quote(string from = null)
        {
            /*
            var table = _database.Tables["user"];

            // first check if the user has opted out
            var dataRows = table.SearchBy("trigger", from);

            var results = new List<TomeMessage>();
            foreach (var dR in dataRows)
                results.Add(new TomeMessage(new DefaultEntry(dR["id"].ToString(), dR["addedBy"].ToString(), dR["trigger"].ToString(), dR["reply"].ToString())));

            if (results != null)
            {
                // we expect there to be only 1 result since there shouldnt be more than 1 entry per user
                if (results[0].Message.CompareTo("true") == 0) return;
            }
            */
            var table = _database.Tables["quote"];

            var entry = table.GetRandomBy("trigger", from);
            
            _database.ReplyCache.Add(entry);
            _chat.SendStatus(Main.chatMain, entry.Message + " -" + from);
        }

        public void ExecuteRepeatCommand(string time)
        {
            var table = _database.Tables["repeat"];

            var entry = table.GetRandomBy("trigger", time);
            if (entry == null) return;

            _database.ReplyCache.Add(entry);
            _chat.SendStatus(Main.chatMain, entry.Message);
        }
    }
}
