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
    public class AdminInfoCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly string _adminChannel;
        private const string RegexString = "@info (.+)"; 

        public AdminInfoCommand(ChatDatabase database, string adminChannel)
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
