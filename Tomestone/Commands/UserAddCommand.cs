using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using Tomestone.Chatting;
using Tomestone.Databases;
using Tomestone.Models;

namespace Tomestone.Commands
{
    public class UserAddCommand : ICommand
    {
        private readonly ChatDatabase _database;

        private const string RegexString = "!add ([a-zA-Z0-9_]+?) (.+)";

        public UserAddCommand(ChatDatabase database)
        {
            _database = database;
        }

        public bool Parse(string message)
        {
            Match match = Regex.Match(message, RegexString);
            return match.Success;
        }

        public string Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            if (match.Success)
            {
                string trigger = match.Groups[1].ToString();
                string reply = match.Groups[2].ToString();

                AddCommandToDatabase(userMessage.From.Nick, trigger, reply);
            }

            return "";
        }

        private void AddCommandToDatabase(string from, string trigger, string reply)
        {
            if (trigger == "quote") return;

            _database.Tables["command"].Insert(from, trigger.ToLower(), reply);
            
            var entry = _database.Tables["command"].GetLatestEntry();
            _database.ReplyCache.Add(entry);
        }
    }
}
