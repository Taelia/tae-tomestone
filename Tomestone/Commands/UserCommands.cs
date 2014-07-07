using TomeLib.Db;
using TomeLib.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomestone.Chatting;
using Tomestone.Databases;
using Tomestone.Models;
using System.Data;

namespace Tomestone.Commands
{
    public class UserCommands
    {
        private Random r = new Random();
        private DateTime getCooldown = DateTime.Now;
        private int _nextCooldown;
        
        private DateTime helpDragonCooldown = DateTime.Now;
        private DateTime helpTomeCooldown = DateTime.Now;
        private DateTime raidCooldown = DateTime.Now;
        private TimeSpan _nextHelpCooldown = TimeSpan.FromMinutes(2);
        

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

            var ok = _database.Tables["reply"].Insert(user, trigger, reply);
            if (ok) _chat.SendStatus(Main.chatMain, "-" + reply + "- succesfully added!");
        }

        public void ExecuteQuoteCommand(string user, string search, string from)
        {
            //Get the latest message from user containing the search string.
            var obj = _chat.ReceivedMessages.Search(user, search);
            if (obj != null)
            {
                var data = new Dictionary<string, string>();
                data.Add("user", obj.From.Nick);
                data.Add("quote", obj.Message);
                data.Add("quotedBy", from);

                // search for a duplicate if it exists
                var results = _database.Tables["quote"].SearchBy("quote", obj.Message);
                if (results == null)
                {
                    var ok = _database.Tables["quote"].Insert(from, obj.From.Nick, obj.Message);
                    if (ok) _chat.SendStatus(Main.chatMain, "-" + obj.Message + "- succesfully quoted!");                    
                }
                return;
            }
            _chat.SendStatus(Main.chatMain, "Message not found.");
        }

        public void ExecuteHelpCommand(string subject)
        {
            switch (subject)
            {
                case "tome":
                    if (DateTime.Now > helpTomeCooldown)
                    {
                        _chat.SendStatus(Main.chatMain, "http://www.simplively.com/blog/2014/02/02/tomestone/");
                        helpTomeCooldown = DateTime.Now + _nextHelpCooldown;
                    }
                    break;
                case "dragon":
                    if (DateTime.Now > helpDragonCooldown)
                    {
                        _chat.SendStatus(Main.chatMain, "http://www.simplively.com/blog/2014/02/02/the-dragon-game/");
                        helpDragonCooldown = DateTime.Now + _nextHelpCooldown;
                    }
                    break;
            }
        }

        public void ExecuteGetCommand(string type, string from = null)
        {
            if (DateTime.Now < getCooldown)
            {
                var percentage = 100 - Math.Round(100 * (getCooldown.Subtract(DateTime.Now).TotalMinutes) / _nextCooldown);
                _chat.SendStatus(Main.chatMain, "Recharging get! " + percentage + "% done.");
                return;
            }

            var table = _database.Tables["command"];

            //Get all commands of type 'type', and then get the id of any random command.
            var entry = table.GetRandomBy("trigger", type);
            if (entry == null)
            {
                _chat.SendStatus(Main.chatMain, "Type not found.");
                return;
            }


            _nextCooldown = r.Next(10, 15);
            getCooldown = DateTime.Now + TimeSpan.FromMinutes(_nextCooldown);

            _database.ReplyCache.Add(entry);
            _chat.SendStatus(Main.chatMain, entry.Message);
        }

        public void ExecuteSpecialCommand(string command)
        {
            if (command.CompareTo("raid") == 0) 
            {
               if(DateTime.Now < raidCooldown) return;
               else raidCooldown = DateTime.Now + _nextHelpCooldown;
            }

            var table = _database.Tables["special"];

            var entry = table.GetRandomBy("trigger", command);

            _database.ReplyCache.Add(entry);
            _chat.SendStatus(Main.chatMain, entry.Message);
        }

        public async void ExecuteHighlightCommand(string from, string description)
        {
            var now = DateTime.Now.ToString("hh:mmtt");
            _chat.SendStatus(Main.chatMain, "Highlight suggested at " + now);

            await Task.Delay(TimeSpan.FromMinutes(30));

            _chat.SendStatus(Main.chatMods, from + " suggested a highlight at " + now + "; " + description);
        }

        public void ExecuteOptoutCommand(string from)
        {
        }

        public void ExecuteOptinCommand(string from)
        {
        }

    }
}
