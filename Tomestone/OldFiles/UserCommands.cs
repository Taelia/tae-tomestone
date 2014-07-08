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

        public void ExecuteOptoutCommand(string from)
        {
        }

        public void ExecuteOptinCommand(string from)
        {
        }

    }
}
