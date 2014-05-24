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
        private IChat _chat;

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
        public void ParseQuote(Channel channel, string message)
        {
            Match match = Regex.Match(message, "!quote (.+?) (.+)");

            if (match.Success)
            {
                string user = match.Groups[1].ToString();
                string search = match.Groups[2].ToString();

                userCommands.ExecuteQuoteCommand(user, search);

                return;
            }

            _chat.SendStatus(Main.chatMain, "SYNTAX: '!quote A B' where A is the username, B is a word within the sentence you want to quote.");
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

        public void ParseGet(Channel channel, string message)
        {
            Match match = Regex.Match(message, "!get (.+)");

            if (match.Success)
            {
                string type = match.Groups[1].ToString();

                userCommands.ExecuteGetCommand(type);
                return;
            }

            _chat.SendStatus(Main.chatMain, "SYNTAX: '!get A' where A is [quote or question].");
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

        public void ParseRepeat()
        {
            var time = DateTime.Now.ToString("hh:mmtt");

            defaultCommands.ExecuteRepeatCommand(time);
        }


        public void Reply(Channel channel, IrcUser from, string message)
        {
            //Only check if a reply is required 25% of the time.
            Random r = new Random();
            if (r.Next(0, 100) < 25)
                defaultCommands.Reply(channel, from, message);
        }
    }
}
