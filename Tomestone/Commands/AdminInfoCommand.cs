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
    public class AdminInfoCommand : BaseAdminCommand
    {
        private readonly ChatDatabase _database;

        protected override string RegexString { get { return "@info (.+)"; } }

        public AdminInfoCommand(ChatDatabase database, string adminChannel) : base(adminChannel)
        {
            _database = database;
        }

        public override TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string search = match.Groups[1].ToString();

            var message = GetInfoFromDatabase(search);
            return new TomeReply(userMessage.Channel, message);
        }

        private string GetInfoFromDatabase(string search)
        {
            var entry = _database.ReplyCache.Last(x => x.Message.Contains(search));

            if (entry == null) 
                return TomeReply.Error();

            return entry.PrintInfo();
        }
    }
}
