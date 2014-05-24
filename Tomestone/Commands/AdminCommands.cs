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
    }
}
