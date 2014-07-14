using System.Collections.Generic;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public class UserTeachCommand : ICommand
    {
        private TomeChat _chat;
        private const string RegexString = "^!teach (.+?) :: (.+)";

        Dictionary<string, Table> Tables = new Dictionary<string, Table>();

        public UserTeachCommand(TomeChat chat)
        {
            _chat = chat;

            Tables.Add("reply", new Table(Database.GetDatabase("tomestone.db"), "replies", "id", "addedBy", "trigger", "reply"));
        }

        public bool Parse(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);
            return match.Success;
        }

        public void Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string trigger = match.Groups[1].Value.ToLower();
            string message = match.Groups[2].Value;

            var reply = AddReplyToDatabase(userMessage.From.Nick, trigger, message);
            _chat.SendMessage(userMessage.Channel.Name, reply);
        }

        private string AddReplyToDatabase(string from, string trigger, string reply)
        {
            //Ignore all Twitch commands.
            if (reply.StartsWith("/timeout") || reply.StartsWith("/ban") ||
                reply.StartsWith("/unban") || reply.StartsWith("/slow") ||
                reply.StartsWith("/slowoff") || reply.StartsWith("/subscribers") ||
                reply.StartsWith("/subscribersoff") || reply.StartsWith("/clear") ||
                reply.StartsWith("/mod") || reply.StartsWith("/unmod") ||
                reply.StartsWith("/r9kbeta") || reply.StartsWith("/r9kbetaoff") ||
                reply.StartsWith("/commercial") || reply.StartsWith("/mods"))
                return DefaultReplies.Angry();


            var data = new Dictionary<string, string>();
            data["addedBy"] = from;
            data["trigger"] = trigger;
            data["reply"] = reply;

            var ok = Tables["reply"].Insert(data);
            if (!ok) return DefaultReplies.Error();

            var entry = Tables["reply"].GetLatestEntry();
            var tomeReply = CreateReply(entry);
            _chat.ReplyCache.Add(tomeReply);

            return DefaultReplies.Confirmation();
        }

        private static TomeReply CreateReply(TableEntry entry)
        {
            var message = entry.Columns["reply"];

            var info = "Reply #" + entry.Columns["id"] + " ( " + entry.Columns["trigger"] + " -> " + entry.Columns["reply"] + " )";

            var tomeReply = new TomeReply(message, info);
            return tomeReply;
        }
    }
}