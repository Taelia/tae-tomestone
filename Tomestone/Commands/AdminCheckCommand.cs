using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Tomestone.Chatting;
using Tomestone.Databases;
using System.Data;

namespace Tomestone.Commands
{
    class AdminCheckCommand : ICommand
    {
        private readonly ChatDatabase _database;
        private readonly TomeChat _chat;
        
        private const string RegexString1 = "@check (.+)";
        private const string RegexString2 = "@check (.+?) (.+)";
        
        public AdminCheckCommand(ChatDatabase database, TomeChat chat)
        {
            _database = database;
            _chat = chat;
        }

        public bool Parse(string message)
        {
            /**** TODO
             * Implement argument checking and parsing a bit more cleanly
             */

            // separate the text into the command to be queried, and additional parameters (if applicable)
            Match match = Regex.Match(message, RegexString1);

            if (!match.Success) match = Regex.Match(message, RegexString2);
            
            return match.Success;

            //_chat.SendStatus(Main.chatMods, "SYNTAX: '@check A B' where A is [commmand, quote or reply] and B is an additional parameter.");
        }
        
        public string Execute(UserMessage userMessage)
        {
            string type = "";
            string args = null;

            Match match = Regex.Match(userMessage.Message, RegexString2);

            if (match.Success) // first case: two arguments (for commands such as reply)
            {
                type = match.Groups[1].Value;
                args = match.Groups[2].Value;
            }
            else // Check if request follows 1 argument syntax
            {
                match = Regex.Match(userMessage.Message, RegexString1);

                if (match.Success)
                {
                    //_chat.SendStatus(Main.chatMods, "currently not functional");
                    type = match.Groups[1].Value;                   
                }
            }

            switch (type)
            {
                case "reply":
                    CheckReplies(args);
                    break;
                case "quote":
                    CheckQuotes(args);
                    break;
                case "command":
                    CheckCommand(args);
                    break;
                case "question":
                    CheckQuestion(args);
                    break;
                case "special":
                    CheckSpecial(args);
                    break;
                case "repeat":
                    CheckRepeat(args);
                    break;
                default:
                    //_chat.SendStatus(Main.chatMods, "Type " + type + " not found.");
                    break;
            }

            return "";
        }

        private void CheckReplies(string args)
        {
            List<DataRow> results = null;
            string message = "";

            var table = _database.Tables["reply"];

            // there are two cases for looking up replies. 
            // 1. if no additional argument is provided, this is treated as a querry for triggers
            // 2. if an addiontal argument is provided, this is treated a querry for all the replies for a specific trigger
            if (args == null || args == "") // Case 1:
            {
                // this should return a list of UNIQUE trigger strings

                results = _database.UserTable.GetDistinctByCol("trigger");
                message = "list of unique triggers: ";

                if (results != null)
                {
                    // return a list of tiggers
                    foreach (var entry in results)
                    {
                        var msg = "uh..";
                        message = message + msg + " | ";
                    }
                    _chat.SendMessage(_chat.ModChannel.Name, message);
                }
                else // no results
                {
                    _chat.SendMessage(_chat.ModChannel.Name, message + " No results to display.");
                }
                return;
            }
            else                            // Case 2:
            {
                // get a list of all replies for the specified trigger (should be in args)
                table.SearchBy("trigger", args);
                message = "list of replies for trigger - " + args + ": ";
            }

            // prepare output and send it
            FormatCheckOutput(message, new List<TableReply>());
        }

        private void CheckQuotes(string args)
        {
            var results = new List<TableReply>();
            string message = "";

            var table = _database.Tables["quote"];

            // Cases for quote seaching:
            // 1. no argument provided: get ALL quotes (not meaningful)
            // 2. search for quotes by username

            if (args == null) // get all quotes, not really a meaningful search
            {
                message = "Please specifiy a username for searching for quotes.";

            }
            else // get quotes by a single username
            {
                results = table.SearchBy("user", args);
                message = "List of quotes for user - " + args + ": ";
            }

            // prepare output and send it
            FormatCheckOutput(message, results);
        }

        private void CheckCommand(string args)
        {
            _chat.SendMessage(_chat.ModChannel.Name, "not implemented");
        }

        private void CheckQuestion(string args)
        {
            _chat.SendMessage(_chat.ModChannel.Name, "not implemented");
        }

        private void CheckSpecial(string args)
        {
            _chat.SendMessage(_chat.ModChannel.Name, "not implemented");
        }

        private void CheckRepeat(string args)
        {
            // for implementation: query by time, or query times

            _chat.SendMessage(_chat.ModChannel.Name, "not implemented");
        }

        private void FormatCheckOutput(string message, List<TableReply> results)
        {
            // format results
            if (results != null)
            {
                //need to format output according to number of results

                if (results.Count == 1)
                {
                    // return detailed info for the entry
                    message += results[0].PrintInfo();
                }
                else if (results.Count <= 10)
                {
                    // return a list of ID+content
                    foreach (TableReply entry in results)
                    {
                        message += entry.PrintInfo() + " | ";
                    }
                }
                else
                {
                    // return a list of IDs
                    foreach (TableReply entry in results)
                    {
                        message = message + entry.Id + " | ";
                    }
                }
                _chat.SendMessage(_chat.ModChannel.Name, message);

            }
            else // no results
            {
                _chat.SendMessage(_chat.ModChannel.Name, message + " No results to display.");
            }
            return;
        }
    }
}
