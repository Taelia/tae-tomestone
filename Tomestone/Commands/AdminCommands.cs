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
            var obj = _chat.SentMessages.Search(search);
            if (obj != null)
            {
                var info = obj.Info();
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
                    var table = _database.GetTableType(type);
                    var obj = _database.GetById(table, id);
                    if (obj != null)
                    {
                        var info = obj.Info();
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
                    var table = _database.GetTableType(type);
                    var ok = _database.Edit(table, id, toReplace, replaceWith);
                    if (ok)
                    {
                        var obj = _database.GetById(table, id);
                        _chat.SendStatus(Main.chatMods, table + " #" + id + "edited: " + obj.Message);
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
                    var table = _database.GetTableType(type);
                    var ok = _database.Delete(table, id);
                    if (ok) _chat.SendStatus(Main.chatMods, type + " #" + id + " deleted.");
                    return;
                default:
                    _chat.SendStatus(Main.chatMods, "Type " + type + " not found.");
                    return;
            }
        }

        public void ExecuteAddCommand(string from, string command, string reply)
        {
            var data = new Dictionary<string, string>();
            data.Add("user", from);
            data.Add("command", command.ToLower());
            data.Add("reply", reply);

            var ok = _database.Insert(TableType.SPECIAL, data);
            if (ok) _chat.SendStatus(Main.chatMods, "-" + reply + "- succesfully added!");
        }

        public void ExecuteRepeatCommand(string from, string time, string message)
        {
            var data = new Dictionary<string, string>();
            data.Add("user", from);
            data.Add("time", time);
            data.Add("message", message);

            var ok = _database.Insert(TableType.REPEAT, data);
            if (ok) _chat.SendStatus(Main.chatMods, "-" + message + "- succesfully added!");
        }

        public void ExecuteCheckCommand(string type, string args)
        {
            List<MessageObject> results;
            string message = "";
            
            switch (type)
            {
                case "reply":
                    // there are two cases for looking up replies. 
                    // 1. if no additional argument is provided, this is treated as a querry for triggers
                    // 2. if an addiontal argument is provided, this is treated a querry for all the replies for a specific trigger
                    if (args == null || args == "") // Case 1:
                    {
                        // this should return a list of UNIQUE trigger strings
                        results = _database.GetDistinctByCol(TableType.REPLY, "trigger");
                        message = "list of unique triggers: ";
                    }
                    else                            // Case 2:
                    {
                        // get a list of all replies for the specified trigger (should be in args)
                        results = _database.SearchBy(TableType.REPLY, "trigger", args);
                        message = "list of replies for trigger - " + args + ": ";
                    }
                    
                    break;

                case "quote":
                    /*
                    var results = _database.SearchBy(TableType.REPLY, "trigger", args);

                    // need to get all quotes, and display them
                    var tableType = _database.GetTable(TableType.QUOTE);

                    // this code gets all the entries in a table and returns them
                    var parms = new Dictionary<string, string>();
                    parms.Add("@TableName", type.TableName);

                    var results = _db.Query("SELECT * FROM @TableName", parms);
                    if (results.Rows.Count == 0) return null;

                    var random = new Random();
                    int r = random.Next(0, results.Rows.Count);

                    var result = results.Rows[r];

                    var obj = CreateObject(table, result);
                    //return obj;
                    return;
                    */
                case "command":
                case "question":
                case "special":
                case "repeat":
                    // for implementation: query by time, or query times
                    
                    
                    _chat.SendStatus(Main.chatMods, "not implemented");
                    return;
                default:
                    _chat.SendStatus(Main.chatMods, "Type " + type + " not found.");
                    return;
            }

            // need to do additional checks here to format output according to number of results.

            // format results
            if (results.Count != 0)
            {                
                var table = _database.GetTable(TableType.REPLY);

                //need to format output according to number of results

                if (results.Count == 1)
                {
                    // return detailed info for the entry
                    message = message + results[0].Info();
                }
                else if (results.Count <= 10)
                {
                    // return a list of ID+reply
                    foreach (MessageObject entry in results)
                    {
                        message = message + entry.Data[table.IdName] + ": " + entry.Message + " | ";
                    }
                }
                else
                {
                    // return a list of IDs
                    foreach (MessageObject entry in results)
                    {
                        message = message + entry.Data[table.IdName] + " | ";
                    }
                }
                _chat.SendStatus(Main.chatMods, message);

            }
            else // no results
            {
                _chat.SendStatus(Main.chatMods, "no results to display for: " + type + ", " + args);
            }
            return;

        }
    }
}
