using System.Data;
using System.Windows.Input;
using TomeLib.Irc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomestone.Chatting;
using Tomestone.Databases;
using Tomestone.Models;

namespace Tomestone.Commands
{
    public class AdminCommands
    {
        private TomeChat _chat;
        private ChatDatabase _database;

        public AdminCommands(TomeChat chat, ChatDatabase database)
        {
            _chat = chat;
            _database = database;
        }

        //searches for a message in sent message history
        public void ExecuteInfoCommand(string search)
        {
            var obj = _database.ReplyCache.Last(x => x.Message.Contains(search));
            if (obj != null)
            {
                var info = obj.PrintInfo();
                _chat.SendStatus(Main.chatMods, info);
                return;
            }
            _chat.SendStatus(Main.chatMods, "Message not found.");
        }

        // fetches a single entry 
        public void ExecuteEntryCommand(string type, string id)
        {
            switch (type)
            {
                case "command":
                case "quote":
                case "question":
                case "reply":
                case "special":
                case "repeat":
                    var entry = _database.Tables[type].GetById(id);
                    if (entry != null)
                    {
                        var info = "";//entry.PrintInfo();
                        _chat.SendStatus(Main.chatMods, info);
                        return;
                    }
                    _chat.SendStatus(Main.chatMods, "Error finding message..");
                    return;
                default:
                    _chat.SendStatus(Main.chatMods, "Type " + type + " not found.");
                    return;
            }
        }

        // edits an existing entry in the database
        public void ExecuteEditCommand(string type, string id, string toReplace, string replaceWith)
        {
            switch (type)
            {
                case "command":
                case "reply":
                case "question":
                case "special":
                case "repeat":
                    var table = _database.Tables[type];
                    var ok = _database.Tables[type].Edit(id, toReplace, replaceWith);
                    if (ok)
                    {
                        var entry = _database.Tables[type].GetById(id);
                        _chat.SendStatus(Main.chatMods, type + " #" + id + "edited: " + ""); //entry.PrintInfo());
                    }
                    return;
                case "quote":
                    _chat.SendStatus(Main.chatMods, "Editing a quote wouldn't make it a quote anymore, silly!");
                    return;
                default:
                    _chat.SendStatus(Main.chatMods, "Type " + type + " not found.");
                    return;
            }
        }

        public void ExecuteDeleteCommand(string type, string id)
        {
            switch (type)
            {
                case "command":
                case "quote":
                case "reply":
                case "question":
                case "special":
                case "repeat":
                    var ok = _database.Tables[type].Delete(id);
                    if (ok) _chat.SendStatus(Main.chatMods, type + " #" + id + " deleted.");
                    return;
                default:
                    _chat.SendStatus(Main.chatMods, "Type " + type + " not found.");
                    return;
            }
        }

        public void ExecuteAddCommand(string from, string command, string reply)
        {
            var ok = _database.Tables["special"].Insert(from, command.ToLower(), reply);
            if (ok) _chat.SendStatus(Main.chatMods, "-" + reply + "- succesfully added!");
        }

        public void ExecuteRepeatCommand(string from, string time, string message)
        {
            var ok = _database.Tables["repeat"].Insert(from, time, message);
            if (ok) _chat.SendStatus(Main.chatMods, "-" + message + "- succesfully added!");
        }

        public void ExecuteCheckCommand(string type, string args)
        {
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
                    _chat.SendStatus(Main.chatMods, "Type " + type + " not found.");
                    break;
            }
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
                    _chat.SendStatus(Main.chatMods, message);
                }
                else // no results
                {
                    _chat.SendStatus(Main.chatMods, message + " No results to display.");
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
            _chat.SendStatus(Main.chatMods, "not implemented");
        }

        private void CheckQuestion(string args)
        {
            _chat.SendStatus(Main.chatMods, "not implemented");
        }

        private void CheckSpecial(string args)
        {
            _chat.SendStatus(Main.chatMods, "not implemented");
        }

        private void CheckRepeat(string args)
        {
            // for implementation: query by time, or query times

            _chat.SendStatus(Main.chatMods, "not implemented");
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
                _chat.SendStatus(Main.chatMods, message);

            }
            else // no results
            {
                _chat.SendStatus(Main.chatMods, message + " No results to display.");
            }
            return;
        }


    }
}
