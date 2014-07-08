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

        private Dictionary<string, DateTime> blacklist;

        public ParseCommands(TomeChat chat, ChatDatabase database)
        {
            _chat = chat;

            adminCommands = new AdminCommands(chat, database);
            userCommands = new UserCommands(chat, database);
            defaultCommands = new DefaultCommands(chat, database);

            blacklist = new Dictionary<string,DateTime>();
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
                case "@tomeout":
                    ParseTomeout(channel, message);
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
        }


        public void ParseCheck(Channel channel, string message)
        {
            /**** TODO
             * Implement argument checking and parsing a bit more cleanly
             */

            // separate the text into the command to be queried, and additional parameters (if applicable)
            Match match = Regex.Match(message, "@check (.+?) (.+)");

            if (match.Success) // first case: two arguments (for commands such as reply)
            {
                string type = match.Groups[1].Value;
                string args = match.Groups[2].Value;
                adminCommands.ExecuteCheckCommand(type, args);

                return;
            }
            else // Check if request follows 1 argument syntax
            {
                match = Regex.Match(message, "@check (.+)");

                if (match.Success)
                {
                    //_chat.SendStatus(Main.chatMods, "currently not functional");
                    string type = match.Groups[1].Value;
                    adminCommands.ExecuteCheckCommand(type, null);

                    return;
                }
            }

            _chat.SendStatus(Main.chatMods, "SYNTAX: '@check A B' where A is [commmand, quote or reply] and B is an additional parameter.");
        }

        // times out the specified user from using tome commands for a specified time
        // assumes that the username is provided as the first argument after check
        // also assumes that the second parameter is a numerical input
        public void ParseTomeout(Channel channel, string message)
        {
            // separate the text into the command to be queried, and additional parameters (if applicable)
            Match match = Regex.Match(message, "@tomeout (.+?) ([0-9]+)");

            if (match.Success)
            {
                string user = match.Groups[1].Value;
                DateTime time = DateTime.Now + TimeSpan.FromMinutes(Double.Parse(match.Groups[2].Value));
                //adminCommands.ExecuteTomeoutCommand(user, time);

                AddUserToBlacklist(user, time);

                return;
            }

            _chat.SendStatus(Main.chatMods, "SYNTAX: '@tomeout A B' where A is a username, and B is a duration in minutes.");
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

        public void AddUserToBlacklist(string username, DateTime duration)
        {
            // only process the command if the user isnt already timed out
            if (!blacklist.ContainsKey(username)) blacklist.Add(username, duration);
        }

        public Boolean CheckBlacklist(string username)
        {
            // check if the specified username is in the blacklist.
            if (blacklist.ContainsKey(username))
            {
                // now check if the timeout has expired
                if (blacklist[username].CompareTo(DateTime.Now) < 0)
                {
                    // remove the user from the timeout list and return false (not timed out)
                    blacklist.Remove(username);
                    return false;
                }
                // still timed out
                return true;
            }
            return false;
        }


    }
}
