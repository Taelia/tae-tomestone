using TomeLib.Db;
using TomeLib.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomestone.Chatting;
using Tomestone.Models;

namespace Tomestone.Commands
{
    public class UserCommands
    {
        private DateTime getCooldown = DateTime.Now;

        private TomeChat _chat;
        private ChatDatabase _database;

        public UserCommands(TomeChat chat, ChatDatabase database)
        {
            _chat = chat;
            _database = database;
        }

        public void ExecuteTeachCommand(string user, string trigger, string command, string reply)
        {
            //Make sure the right commands have been used.
            if (!(command == "reply" || command == "action")) return;
            if (command == "action") reply = "/me " + reply;

            //Ignore all Twitch commands.
            if (reply.StartsWith("/timeout") || reply.StartsWith("/ban") ||
                reply.StartsWith("/unban") || reply.StartsWith("/slow") ||
                reply.StartsWith("/slowoff") || reply.StartsWith("/subscribers") ||
                reply.StartsWith("/subscribersoff") || reply.StartsWith("/clear") ||
                reply.StartsWith("/mod") || reply.StartsWith("/unmod") ||
                reply.StartsWith("/r9kbeta") || reply.StartsWith("/r9kbetaoff") ||
                reply.StartsWith("/commercial") || reply.StartsWith("/mods")
                ) return;

            //Create data object to put into the database.
            var data = new Dictionary<string, string>();
            data.Add("user", user);
            data.Add("trigger", trigger);
            data.Add("reply", reply);

            var ok = _database.Insert(TableType.REPLY, data);
            if (ok) _chat.SendStatus(Main.chatMain, "-" + reply + "- succesfully added!");
        }

        public void ExecuteQuoteCommand(string user, string search)
        {
            //Get the latest message from user containing the search string.
            var obj = _chat.ReceivedMessages.Search(user, search);
            if (obj != null)
            {
                var data = new Dictionary<string, string>();
                data.Add("user", obj.From.Nick);
                data.Add("quote", obj.Message);

                var ok = _database.Insert(TableType.QUOTE, data);
                if (ok) _chat.SendStatus(Main.chatMain, "-" + obj.Message + "- succesfully quoted!");
                return;
            }
            _chat.SendStatus(Main.chatMain, "Message not found.");
        }

        public void ExecuteHelpCommand(string subject)
        {
            switch (subject)
            {
                case "tome":
                    _chat.SendStatus(Main.chatMain, "http://www.simplively.com/blog/2014/02/02/tomestone/");
                    break;
                case "dragon":
                    _chat.SendStatus(Main.chatMain, "http://www.simplively.com/blog/2014/02/02/the-dragon-game/");
                    break;
            }
        }

        public void ExecuteGetCommand(string type)
        {
            if (DateTime.Now < getCooldown + TimeSpan.FromMinutes(2))
            {
                var percentage = Math.Round(100 * DateTime.Now.Subtract(getCooldown).TotalMinutes / 2);
                _chat.SendStatus(Main.chatMain, "Recharging get! " + percentage + "% done.");
                return;
            }

            MessageObject obj = null;
            switch (type)
            {
                case "quote":
                    obj = _database.GetRandom(TableType.QUOTE);
                    break;
                default:
                    //Get all commands of type 'type', and then get the id of any random command.
                    obj = _database.GetRandomBy(TableType.COMMAND, "command", type);
                    if (obj == null)
                    {
                        _chat.SendStatus(Main.chatMain, "Type not found.");
                        return;
                    }
                    break;
            }

            getCooldown = DateTime.Now;
            _chat.SentMessages.Add(obj);
            _chat.SendStatus(Main.chatMain, obj.Message);
        }

        public void ExecuteAddCommand(string from, string command, string reply)
        {
            if (command == "quote")
            {
                _chat.SendStatus(Main.chatMain, "Use the !quote command to quote people!");
                return;
            }

            var data = new Dictionary<string, string>();
            data.Add("user", from);
            data.Add("command", command.ToLower());
            data.Add("reply", reply);

            var ok = _database.Insert(TableType.COMMAND, data);
            if (ok) _chat.SendStatus(Main.chatMain, "-" + reply + "- succesfully added!");
        }

        public void ExecuteSpecialCommand(string command)
        {
            var obj = _database.GetRandomBy(TableType.SPECIAL, "command", command);

            _chat.SentMessages.Add(obj);
            _chat.SendStatus(Main.chatMain, obj.Message);
        }

        public async void ExecuteHighlightCommand(string from, string description)
        {
            var now = DateTime.Now.ToString("hh:mmtt");
            _chat.SendStatus(Main.chatMain, "Highlight suggested at " + now);

            await Task.Delay(TimeSpan.FromMinutes(30));

            _chat.SendStatus(Main.chatMods, from + " suggested a highlight at " + now + "; " + description);
        }
        
    }
}
