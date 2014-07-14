using System.Collections.Generic;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class AdminAddCommand : ICommand
    {
        private readonly TomeChat _chat;

        private const string RegexString = "^@add ([a-zA-Z0-9_]+?) (.+)";

        private readonly Table _commandTable = new Table(Database.GetDatabase("tomestone.db"), "commands", "id", "addedBy", "trigger", "reply");

        public AdminAddCommand(TomeChat chat)
        {
            _chat = chat;
        }

        public bool Parse(UserMessage message)
        {
            Match match = Regex.Match(message.Message, RegexString);
            var isAdminChannel = message.Channel.Name == _chat.Channels["mods"];

            return match.Success && isAdminChannel;
        }

        public void Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string addedBy = userMessage.From.Nick;
            string trigger = match.Groups[1].ToString();
            string reply = match.Groups[2].ToString();

            var message = AddCommandToDatabase(addedBy, trigger, reply);
            _chat.SendMessage(userMessage.Channel.Name, "/me :: " + message);
        }

        private string AddCommandToDatabase(string from, string trigger, string reply)
        {
            //Create data to be added into the database
            var data = new Dictionary<string, string>();
            data["addedBy"] = from;
            data["trigger"] = trigger.ToLower();
            data["reply"] = reply;

            var ok = _commandTable.Insert(data);
            if (!ok) return DefaultReplies.Error();

            //Create TomeMessage to log in the history.
            var entry = _commandTable.GetLatestEntry();
            var tomeReply = CreateReply(entry);
            _chat.ReplyCache.Add(tomeReply);

            return "Command !" + trigger + " has been created succesfully.";
        }

        private static TomeReply CreateReply(TableEntry entry)
        {
            var message = entry.Columns["reply"];
            var info = "Command #" + entry.Columns["id"] + " ( " + entry.Columns["trigger"] + " -> " + entry.Columns["reply"] + " )";

            var tomeReply = new TomeReply(message, info);
            return tomeReply;
        }
    }
}
