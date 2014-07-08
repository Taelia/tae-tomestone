using System.Collections.Generic;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using Tomestone.Chatting;
using Tomestone.Databases;

namespace Tomestone.Commands
{
    public class UserTeachCommand : ICommand
    {
        private readonly ChatDatabase _database;

        private const string RegexString = "!teach (.+?) :: (.+)";

        public UserTeachCommand(ChatDatabase database)
        {
            _database = database;
        }

        public bool Parse(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);
            return match.Success;
        }

        public TomeReply Execute(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);

            string trigger = match.Groups[1].Value.ToLower();
            string reply = match.Groups[2].Value;

            var message = AddReplyToDatabase(userMessage.From.Nick, trigger, reply);
            return new TomeReply(userMessage.Channel, message);
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
                return TomeReply.Angry();


            var ok = _database.Tables["reply"].Insert(from, trigger.ToLower(), reply);
            if (!ok)
                return TomeReply.Error();

            var entry = _database.Tables["reply"].GetLatestEntry();
            _database.ReplyCache.Add(entry);

            return TomeReply.Confirmation();
        }
    }
}