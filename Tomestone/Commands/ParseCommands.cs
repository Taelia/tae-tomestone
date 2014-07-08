using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net;
using Tomestone.Chatting;
using Tomestone.Databases;
using Tomestone.Models;

namespace Tomestone.Commands
{
    public class ParseCommands
    {
        private TomeChat _chat;

        private AdminCommands adminCommands;
        private UserCommands userCommands;
        private DefaultCommands defaultCommands;

        public ParseCommands(TomeChat chat, ChatDatabase database)
        {
            _chat = chat;

            adminCommands = new AdminCommands(chat, database);
            userCommands = new UserCommands(chat, database);
            defaultCommands = new DefaultCommands(chat, database);
        }

        public void ParseAdminCommands(Channel channel, IrcUser from, string message, string command)
        {
            // only receive commands sent from mod channel
            if (channel.Name != Main.chatMods) return;

            switch (command)
            {
                case "@check":
                    ParseCheck(channel, message);
                    break;
            }
        }

        public void ParseUserCommands(Channel channel, IrcUser from, string message, string command)
        {
            //if (channel.Name != Main.chatMain) return;

            // ignore commands from blacklisted users
            if (CheckBlacklist(from.Nick)) return;

            switch (command)
            {
                case "!optout":
                    ParseOptout(channel, from);
                    break;
                case "!optin":
                    ParseOptin(channel, from);
                    break;
                default:
                    ParseSpecial(channel, from, message);
                    break;
            }
        }

        public void ParseDefaultCommands(Channel channel, IrcUser from, string message, string command)
        {
            Quote(channel, from, message);
            Reply(channel, from, message);
        }

        //**** ADMIN COMMANDS ****

        public void ParseRepeat()
        {
            var time = DateTime.Now.ToString("hh:mmtt");

            defaultCommands.ExecuteRepeatCommand(time);
        }


        //**** USER COMMANDS ****

        public void ParseOptout(Channel channel, IrcUser from)
        {
            userCommands.ExecuteOptoutCommand(from.Nick);
        }

        public void ParseOptin(Channel channel, IrcUser from)
        {
            userCommands.ExecuteOptinCommand(from.Nick);
        }

        public void ParseSpecial(Channel channel, IrcUser from, string message)
        {
            Match match = Regex.Match(message, "!([a-zA-Z0-9_]+)$");

            if (match.Success)
            {
                string command = match.Groups[1].ToString();

                userCommands.ExecuteSpecialCommand(command);
                return;
            }
        }

        public void Reply(Channel channel, IrcUser from, string message)
        {
            //Only check if a reply is required 25% of the time.
            Random r = new Random();
            if (r.Next(0, 100) < 25)
                defaultCommands.Reply(channel, from, message);
        }

        public void Quote(Channel channel, IrcUser from, string message)
        {
            //chance to quote is 2%
            Random r = new Random();
            if (r.Next(0, 100) < 1) 
            {

                string user = from.Nick.ToLower();

                defaultCommands.Quote(user);
            }
        }

        


    }
}
