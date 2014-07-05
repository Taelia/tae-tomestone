using TomeLib.Db;
using TomeLib.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomestone.Chatting;
using Tomestone.Models;
using System.Data;

namespace Tomestone.Commands
{
    public class UserCommands
    {
        private Random r = new Random();
        private DateTime getCooldown = DateTime.Now;
        private int _nextCooldown;

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

        public void ExecuteQuoteCommand(string user, string search, string by)
        {
            if (user == by)
            {
                _chat.SendStatus(Main.chatMain, "Don't quote yourself, silly.");
                return;
            }
            //Get the latest message from user containing the search string.
            var obj = _chat.ReceivedMessages.Search(user, search);
            if (obj != null)
            {
                //!!! SuperQuoteCommand just coincidentally happens to have this exact piece of code. Be careful when changing.
                ExecuteSuperQuoteCommand(obj.From.Nick, obj.Message, by);
                return;
            }
            _chat.SendStatus(Main.chatMain, "Message not found.");
        }

        //!!! Read comment in ExecuteQuoteCommand. This bit may require cleaner code.
        public void ExecuteSuperQuoteCommand(string user, string quote, string by)
        {
            var data = new Dictionary<string, string>();
            data.Add("user", user);
            data.Add("quote", quote);
            data.Add("quotedBy", by);

            var ok = _database.Insert(TableType.QUOTE, data);
            if (ok)
            {
                var newObj = _database.NewestEntry(TableType.QUOTE);
                _chat.SentMessages.Add(newObj);
                _chat.SendStatus(Main.chatMain, "-" + quote + "- succesfully quoted!");
            }
            return;
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

        //This method really requires rewriting.
        public void ExecuteGetCommand(string type, string from = null)
        {
            if (DateTime.Now < getCooldown)
            {
                var percentage = 100 - Math.Round(100 * (getCooldown.Subtract(DateTime.Now).TotalMinutes) / _nextCooldown);
                _chat.SendStatus(Main.chatMain, "Recharging get! " + percentage + "% done.");
                return;
            }

            ChatMessage obj = null;
            switch (type)
            {
                case "quote":
                    obj = _database.GetRandomBy(TableType.QUOTE, "user", from);
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

            _nextCooldown = r.Next(3, 7);
            if (type == "quote" && obj == null)
            {
                _chat.SendStatus(Main.chatMain, from + " has no quotes.");
                return;
            }
            getCooldown = DateTime.Now + TimeSpan.FromMinutes(_nextCooldown);

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
            if (ok)
            {
                var obj = _database.NewestEntry(TableType.COMMAND);
                _chat.SentMessages.Add(obj);
                _chat.SendStatus(Main.chatMain, "-" + reply + "- succesfully added!");
            }
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
