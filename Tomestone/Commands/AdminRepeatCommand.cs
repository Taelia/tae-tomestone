using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminRepeatCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly TomeChat _chat;

        private const string RegexString = "@repeat (([01][0-9]|2[0-3]):[0-5][0-9][ap]m) (.+)";

        public AdminRepeatCommand(ChatDatabase database, TomeChat chat)
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

                AddRepeatToDatabase(userMessage.From.Nick, trigger, reply);
            }

            return "";
        }

        private void AddRepeatToDatabase(string from, string trigger, string reply)
        {
            _database.Tables["repeat"].Insert(from, trigger.ToLower(), reply);

            var entry = _database.Tables["repeat"].GetLatestEntry();
            _database.ReplyCache.Add(entry);
        }
    }
}