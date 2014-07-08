using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Meebey.SmartIrc4net;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class AdminAddCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly string _adminChannel;
        private const string RegexString = "@add ([a-zA-Z0-9_]+?) (.+)";

        public AdminAddCommand(ChatDatabase database, string adminChannel)
        {
            _database = database;
            _adminChannel = adminChannel;
        }

        public bool Parse(UserMessage message)
        {
            Match match = Regex.Match(message.Message, RegexString);
            var isAdminChannel = message.Channel.Name == _adminChannel;

            return match.Success && isAdminChannel;
        }

        public TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string addedBy = userMessage.From.Nick;
            string trigger = match.Groups[1].ToString();
            string reply = match.Groups[2].ToString();

            var message = AddSpecialCommandToDatabase(addedBy, trigger, reply);
            return new TomeReply(userMessage.Channel, message);
        }

        private string AddSpecialCommandToDatabase(string from, string trigger, string reply)
        {
            var ok = _database.Tables["special"].Insert(from, trigger.ToLower(), reply);
            if (!ok) 
                return TomeReply.Error();

            var entry = _database.Tables["special"].GetLatestEntry();
            _database.ReplyCache.Add(entry);

            return TomeReply.Confirmation();
        }
    }
}
