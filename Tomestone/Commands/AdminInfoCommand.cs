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
        private readonly TomeChat _chat;

        private const string RegexString = "@info (.+)";

        public AdminInfoCommand(ChatDatabase database, TomeChat chat)
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
                string search = match.Groups[1].ToString();

                FindAndPrintInfo(search);
            }

            return "";
        }

        private void FindAndPrintInfo(string search)
        {
            var entry = _database.ReplyCache.Last(x => x.Message.Contains(search));

            if (entry != null)
            {
                var info = entry.PrintInfo();
                _chat.SendStatus(_chat.ModChannel.Name, info);
            }
        }
    }
}
