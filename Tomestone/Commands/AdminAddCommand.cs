using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminAddCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly TomeChat _chat;

        private const string RegexString = "@add ([a-zA-Z0-9_]+?) (.+)";

        public AdminAddCommand(ChatDatabase database, TomeChat chat)
        {
            _database = database;
            _chat = chat;
        }

        public bool Parse(string message)
        {
            Match match = Regex.Match(message, RegexString);
            return match.Success;
        }

        public string Execute(UserMessage userMessage)
        {
            if (userMessage.Channel != _chat.ModChannel)
                return "";

            Match match = Regex.Match(userMessage.Message, RegexString);

            if (match.Success)
            {
                string trigger = match.Groups[1].ToString();
                string reply = match.Groups[2].ToString();

                AddSpecialCommandToDatabase(userMessage.From.Nick, trigger, reply);
            }

            return "";
        }

        private void AddSpecialCommandToDatabase(string from, string trigger, string reply)
        {
            _database.Tables["special"].Insert(from, trigger.ToLower(), reply);

            var entry = _database.Tables["special"].GetLatestEntry();
            _database.ReplyCache.Add(entry);
        }
    }
}
