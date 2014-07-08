using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class UserAddReply : ICommand
    {
        private ChatDatabase _database;

        Random r = new Random();
        private DateTime _lastUsed;

        public UserAddReply(ChatDatabase database)
        {
            _database = database;
        }

        public bool Parse(UserMessage message)
        {
            Match match = Regex.Match(message.Message, "!([a-zA-Z0-9_]+)$");
            var cooldownPassed = _lastUsed < DateTime.Now - TimeSpan.FromSeconds(15);

            return match.Success && cooldownPassed;
        }

        public TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, "!([a-zA-Z0-9_]+)$");

            string command = match.Groups[1].ToString();

            var message = GetCommandFromDatabase(command);
            return new TomeReply(userMessage.Channel, message);
        }

        private string GetCommandFromDatabase(string command)
        {
            var table = _database.Tables["special"];

            var entry = table.GetRandomBy("trigger", command);
            if (entry == null)
                return TomeReply.Error();

            _database.ReplyCache.Add(entry);
            _lastUsed = DateTime.Now;
            return entry.PrintMessage();
        }
    }
}