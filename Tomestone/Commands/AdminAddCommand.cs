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
    public class AdminAddCommand : BaseAdminCommand
    {
        private readonly ChatDatabase _database;
        protected override string RegexString { get { return "@add ([a-zA-Z0-9_]+?) (.+)"; } }

        public AdminAddCommand(ChatDatabase database, string adminChannel)
            : base(adminChannel)
        {
            _database = database;
        }

        public override TomeReply Execute(UserMessage userMessage)
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
