using TomeLib.Db;
using TomeLib.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tomestone;
using Tomestone.Commands;
using Meebey.SmartIrc4net;
using Tomestone.Models;

namespace Tomestone.Chatting
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
                case "@info":
                    ParseInfo(channel, message);
                    break;
                case "@delete":
                    ParseDelete(channel, message);
                    break;
                case "@edit":
                    ParseEdit(channel, message);
                    break;
                case "@entry":
                    ParseEntry(channel, message);
                    break;
                case "@add":
                    ParseSpecialAdd(channel, from, message);
                    break;
                case "@repeat":
                    ParseRepeat(channel, from, message);
                    break;
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
            if (channel.Name != Main.chatMain) return;

            // ignore commands from blacklisted users
            if (CheckBlacklist(from.Nick)) return;

            switch (command)
            {
                case "!teach":
                    ParseTeach(channel, from, message);
                    break;
                case "!quote":
                    ParseQuote(channel, from, message);
                    break;
                case "!help":
                    ParseHelp(channel, message);
                    break;
                case "!get":
                    ParseGet(channel, message);
                    break;
                case "!add":
                    ParseAdd(channel, from, message);
                    break;
                case "!highlight":
                    ParseHighlight(channel, from, message);
                    break;
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

        public void ParseInfo(Channel channel, string message)
        {
            Match match = Regex.Match(message, "@info (.+)");

            if (match.Success)
            {
                string search = match.Groups[1].Value;

                adminCommands.ExecuteInfoCommand(search);

                return;
            }

            _chat.SendStatus(Main.chatMods, "SYNTAX: '@info A' where A is a unique word from the sentence you want info on.");
        }

        public void ParseDelete(Channel channel, string message)
        {
            Match match = Regex.Match(message, "@delete (.+?) (.+)");

            if (match.Success)
            {
                string type = match.Groups[1].Value;
                string id = match.Groups[2].Value;

                adminCommands.ExecuteDeleteCommand(type, id);

                return;
            }

            _chat.SendStatus(Main.chatMods, "SYNTAX: '@delete A B' where A is [commmand, quote or reply] and B is the id-number.");
        }

        public void ParseEdit(Channel channel, string message)
        {
            Match match = Regex.Match(message, "@edit (.+?) (.+?) (.+?)=(.+)");

            if (match.Success)
            {
                string type = match.Groups[1].Value;
                string id = match.Groups[2].Value;
                string toReplace = match.Groups[3].Value;
                string replaceWith = match.Groups[4].Value;

                adminCommands.ExecuteEditCommand(type, id, toReplace, replaceWith);

                return;
            }

            _chat.SendStatus(Main.chatMods, "SYNTAX: '@edit A B C=D' where A is [commmand, quote or reply], B is the id-number, C is the word you want replaced, and D is what you want to replace it with.");
        }

        public void ParseEntry(Channel channel, string message)
        {
            Match match = Regex.Match(message, "@entry (.+?) (.+)");

            if (match.Success)
            {
                string type = match.Groups[1].Value;
                string id = match.Groups[2].Value;

                adminCommands.ExecuteEntryCommand(type, id);

                return;
            }

            _chat.SendStatus(Main.chatMods, "SYNTAX: '@entry A B' where A is [commmand, quote or reply] and B is the id-number.");
        }

        public void ParseSpecialAdd(Channel channel, IrcUser from, string message)
        {
            Match match = Regex.Match(message, "@add ([a-zA-Z0-9_]+?) (.+)");

            if (match.Success)
            {
                string command = match.Groups[1].ToString();
                string reply = match.Groups[2].ToString();

                adminCommands.ExecuteAddCommand(from.Nick, command, reply);
                return;
            }

            _chat.SendStatus(Main.chatMods, "SYNTAX: '@add A B' where A is a single word, and B is the message you want to enter.");
        }

        public void ParseRepeat(Channel channel, IrcUser from, string message)
        {
            Match match = Regex.Match(message, "@repeat (([01][0-9]|2[0-3]):[0-5][0-9][ap]m) (.+)");
            if (match.Success)
            {
                string time = match.Groups[1].ToString();
                string repeat = match.Groups[3].ToString();

                adminCommands.ExecuteRepeatCommand(from.Nick, time, repeat);
                return;
            }

            _chat.SendStatus(Main.chatMods, "SYNTAX: '@repeat A B' where A is a timestamp [hh:mmtt] (e.g. 02:05am), and B is the message you want to enter.");
        }

        public void ParseRepeat()
        {
            var time = DateTime.Now.ToString("hh:mmtt");

            defaultCommands.ExecuteRepeatCommand(time);
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

        /// <summary>
        /// Allow a user to teach Tomestone new replies.
        /// </summary>
        public void ParseTeach(Channel channel, IrcUser from, string message)
        {
            //Match sentences like "!teach apples <reply> oranges."
            Match match = Regex.Match(message, "!teach (.+?) <(.+?)> (.+)");

            if (match.Success)
            {
                //Make the trigger and command lowercase before storing in the database.
                string trigger = match.Groups[1].Value.ToLower();
                string command = match.Groups[2].Value.ToLower();
                string reply = match.Groups[3].Value;

                userCommands.ExecuteTeachCommand(from.Nick, trigger, command, reply);
                return;
            }

            _chat.SendStatus(Main.chatMain, "SYNTAX: '!teach A <B> C' where A is the trigger, and B is [reply or action] and C is the reply.");
        }

        /// <summary>
        /// Allow user to store a quote from someone.
        /// </summary>
        public void ParseQuote(Channel channel, IrcUser from, string message)
        {
            Match match = Regex.Match(message, "!quote (.+?) (.+)");

            if (match.Success)
            {
                string user = match.Groups[1].ToString();
                string search = match.Groups[2].ToString();

                userCommands.ExecuteQuoteCommand(user, search, from.Nick);

                return;
            }

            _chat.SendStatus(Main.chatMain, "SYNTAX: '!quote A B' where A is the username, B is a word within the sentence you want to quote.");
        }

        /// <summary>
        /// Allow user to store a quote from the streamer.
        /// </summary>
        public void ParseSuperQuote(Channel channel, IrcUser from, string message)
        {
            Match match = Regex.Match(message, "!superquote (.+)");

            if (match.Success)
            {
                string quote = match.Groups[1].ToString();

                userCommands.ExecuteQuoteCommand(channel.Name.Substring(1), quote, from.Nick);

                return;
            }

            _chat.SendStatus(Main.chatMain, "SYNTAX: '!superquote A' where A is something the streamer said.");
        }

        public void ParseHelp(Channel channel, string message)
        {
            Match match = Regex.Match(message, "!help (.+)");

            if (match.Success)
            {
                var subject = match.Groups[1].Value.ToLower();

                userCommands.ExecuteHelpCommand(subject);
                return;

            }
            _chat.SendStatus(Main.chatMain, "SYNTAX: '!get A' where A is [tome or dragon].");
        }

        //This method requires re-writing. Perhaps splitting !get quote and !get X completely.
        public void ParseGet(Channel channel, string message)
        {
            //First check if the user wants a quote.
            Match match = Regex.Match(message, "!get quote (.+)");

            if (match.Success)
            {
                
                //string user = match.Groups[1].ToString().ToLower();

                //userCommands.ExecuteGetCommand("quote", user);

                _chat.SendStatus(Main.chatMain, "This function has been removed. Tome will now display quotes randomly");
                return;
            }
            else
            {
                //If the user doesn't want a quote, check what it does want.
                match = Regex.Match(message, "!get (.+)");

                if (match.Success)
                {
                    string type = match.Groups[1].ToString();
                    
                    //If the user wants a quote but didn't supply a username, give the Syntax message.
                    if (type != "quote")
                    {
                        userCommands.ExecuteGetCommand(type);
                        return;
                    }
                }
            }

            _chat.SendStatus(Main.chatMain, "SYNTAX: '!get A' where A is an !add-ed command, or '!get quote B' where B is a username.");
        }

        public void ParseAdd(Channel channel, IrcUser from, string message)
        {
            Match match = Regex.Match(message, "!add ([a-zA-Z0-9_]+?) (.+)");

            if (match.Success)
            {
                string command = match.Groups[1].ToString();
                string reply = match.Groups[2].ToString();

                userCommands.ExecuteAddCommand(from.Nick, command, reply);
                return;
            }

            _chat.SendStatus(Main.chatMain, "SYNTAX: '!add A B' where A is a single word, and B is the message you want to enter.");
        }

        public void ParseHighlight(Channel channel, IrcUser from, string message)
        {
            Match match = Regex.Match(message, "!highlight (.+)");

            if (match.Success)
            {
                string description = "";
                if (match.Groups.Count > 0)
                    description = match.Groups[1].ToString();

                userCommands.ExecuteHighlightCommand(from.Nick, description);
                return;
            }

            _chat.SendStatus(Main.chatMain, "SYNTAX: '!highlight A' where A is a short description of the highlight you want to suggest.");
        }

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
            if (r.Next(0, 100) < 2) 
            {

                string user = from.Nick.ToLower();

                defaultCommands.Quote(user);
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
