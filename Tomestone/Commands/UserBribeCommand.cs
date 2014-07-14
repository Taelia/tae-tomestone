﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class UserBribeCommand : ICommand
    {
        private readonly TomeChat _chat;

        readonly Dictionary<string, Table> _tables = new Dictionary<string, Table>();

        private const string RegexString = @"^!bribe (.+?) (\d+)";

        public UserBribeCommand(TomeChat chat)
        {
            _chat = chat;

            _tables.Add("bribe", new Table(Database.GetDatabase("tomestone.db"), "votes", "id", "name", "count", "vote"));
            _tables.Add("dragon", new Table(Database.GetDatabase("dragon.db"), "dragon", "dragonId", "user", "coins"));
        }

        public bool Parse(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);
            return match.Success;
        }

        public void Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string name = match.Groups[1].ToString();
            int count = int.Parse(match.Groups[2].ToString());

            var reply = AddBribeToDatabase(userMessage.From.Nick, name, count);
            _chat.SendMessage(userMessage.Channel.Name, reply);
        }

        private string AddBribeToDatabase(string from, string name, int count)
        {
            var entryList = _tables["bribe"].SearchBy("name", name);
            if (entryList == null)
            {
                var data = new Dictionary<string, string>();
                data.Add("name", name);
                _tables["bribe"].Insert(data);
                entryList = _tables["bribe"].SearchBy("name", name);
            }
            var entry = entryList.FirstOrDefault();
            if (entry == null) return DefaultReplies.Denial();

            //If an entry is defined as 'votable', deny.
            if (entry.Columns["vote"] != "False") return DefaultReplies.Denial();

            var userList = _tables["dragon"].SearchBy("user", from);
            if (userList == null) return DefaultReplies.Denial();
            var user = userList.FirstOrDefault();
            if (user == null) return DefaultReplies.Denial();

            //If user has less coins than he wants to enter, deny.
            var coins = int.Parse(user.Columns["coins"]);
            if (coins < count) return DefaultReplies.Denial();

            var newCoins = coins - count;

            var currentCount = int.Parse(entry.Columns["count"]);
            var newCount = currentCount + count;

            var ok = _tables["bribe"].Update(entry.Columns["id"], "count", newCount.ToString(CultureInfo.InvariantCulture));
            if (!ok) return DefaultReplies.Error();
            ok = _tables["dragon"].Update(user.Columns["dragonId"], "coins", newCoins.ToString(CultureInfo.InvariantCulture));
            if (!ok) return DefaultReplies.Error();

            return DefaultReplies.Confirmation();
        }
    }
}
