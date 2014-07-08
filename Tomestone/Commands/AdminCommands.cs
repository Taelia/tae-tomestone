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

    }
}
