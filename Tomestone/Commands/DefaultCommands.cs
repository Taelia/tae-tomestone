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

        private MessageObject PickRandomReply(List<MessageObject> results)
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

            var results = _database.SearchBy(TableType.REPLY, "trigger", search);
            if (results == null) return;

            var obj = PickRandomReply(results);
            if (obj == null) return;

            _chat.SentMessages.Add(obj);

            //Replace wildcards with their corresponing replacements.
            var reply = obj.Message.Replace("%who", from.Nick);

            //Finally, send the message.
            _chat.SendMessage(channel.Name, reply);
            _chat.ReceivedMessages.Add(new MessageObject(from, message));
        }

        public void Quote(string from = null)
        {
            MessageObject obj = null;

            // first check if the user has opted out
            var results = _database.SearchBy(TableType.USER, "user", from);

            if (results != null)
            {
                // we expect there to be only 1 result since there shouldnt be more than 1 entry per user
                if (results[0].Message.CompareTo("true") == 0) return;
            }

            obj = _database.GetRandomBy(TableType.QUOTE, "user", from);
            
            _chat.SentMessages.Add(obj);
            _chat.SendStatus(Main.chatMain, obj.Message);
        }

        public void ExecuteRepeatCommand(string time)
        {
            var obj = _database.GetRandomBy(TableType.REPEAT, "time", time);
            if (obj == null) return; 

            _chat.SentMessages.Add(obj);
            _chat.SendStatus(Main.chatMain, obj.Message);
        }
    }
}
