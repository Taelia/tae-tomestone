using TomeLib.Irc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tomestone.Chatting;
using TomeLib.Db;

namespace Tomestone
{
    public enum TableType { REPEAT, SPECIAL, COMMAND, REPLY, QUOTE, QUESTION, ERROR };

    public class ChatDatabase
    {
        private Database _db;

        public ChatDatabase(Database db)
        {
            _db = db;

            
        }

        public Table GetTable(TableType table)
        {
            switch (table)
            {
                case TableType.REPEAT: return new RepeatTable();
                case TableType.SPECIAL: return new SpecialTable();
                case TableType.COMMAND: return new CommandTable();
                case TableType.REPLY: return new ReplyTable();
                case TableType.QUOTE: return new QuoteTable();
                case TableType.QUESTION: return new QuestionTable();
            }
            return null;
        }
        public TableType GetTableType(string table)
        {
            switch (table)
            {
                case "repeat": return TableType.REPEAT;
                case "special": return TableType.SPECIAL;
                case "command": return TableType.COMMAND;
                case "reply": return TableType.REPLY;
                case "quote": return TableType.QUOTE;
                case "question": return TableType.QUESTION;
            }
            return TableType.ERROR;
        }

        public bool Insert(TableType table, Dictionary<string, string> data)
        {
            var type = GetTable(table);

            var ok = _db.Insert(type.TableName, data);
            if (!ok) return false;
            return true;
        }

        public bool Edit(TableType table, string id, string toReplace, string replaceWith)
        {
            if (toReplace == replaceWith) return false;
            var type = GetTable(table);

            var obj = GetById(table, id);
            if (obj == null) return false;

            //Edit the message
            obj.Message = obj.Message.Replace(toReplace, replaceWith);

            var data = new Dictionary<string, string>();
            data.Add(type.EditName, obj.Message);

            var parms = new Dictionary<string, string>();
            parms.Add("@IdName", type.IdName);
            parms.Add("@Id", id);

            var ok = _db.Update(type.TableName, data, "@IdName = '@Id'", parms);
            if (!ok) return false;
            return true;
        }

        public bool Delete(TableType table, string id)
        {
            var type = GetTable(table);

            var parms = new Dictionary<string, string>();
            parms.Add("@IdName", type.IdName);
            parms.Add("@Id", id);

            var ok = _db.Delete(type.TableName, "@IdName = '@Id'", parms);
            if (!ok) return false;

            return true;
        }

        public MessageObject GetById(TableType table, string id)
        {
            var type = GetTable(table);

            //Get the message to be edited
            var parms = new Dictionary<string, string>();
            parms.Add("@TableName", type.TableName);
            parms.Add("@IdName", type.IdName);
            parms.Add("@Id", id);

            var results = _db.Query("SELECT * FROM @TableName WHERE @IdName = '@Id'", parms);
            if (results.Rows.Count == 0) return null;

            var result = results.Rows[0];

            return CreateObject(table, result);
        }

        public MessageObject NewestEntry(TableType table)
        {
            var type = GetTable(table);

            var parms = new Dictionary<string, string>();
            parms.Add("@TableName", type.TableName);
            parms.Add("@IdName", type.IdName);

            var results = _db.Query("SELECT * FROM @TableName ORDER BY @IdName DESC", parms);
            if (results.Rows.Count == 0) return null;
            var result = results.Rows[0];

            var id = result[type.IdName].ToString();

            var obj = CreateObject(table, result);
            return obj;
        }

        public MessageObject GetRandom(TableType table)
        {
            var type = GetTable(table);

            var parms = new Dictionary<string, string>();
            parms.Add("@TableName", type.TableName);

            var results = _db.Query("SELECT * FROM @TableName", parms);
            if (results.Rows.Count == 0) return null;

            var random = new Random();
            int r = random.Next(0, results.Rows.Count);

            var result = results.Rows[r];

            var obj = CreateObject(table, result);
            return obj;
        }

        // returns all entries in a table based on a column type and a text field
        public List<MessageObject> SearchBy(TableType table, string columnName, string search)
        {
            var type = GetTable(table);

            //First check for the message as is.
            search = search.ToLower();
            var parms = new Dictionary<string, string>();
            parms.Add("@TableName", type.TableName);
            parms.Add("@Trigger", columnName);
            parms.Add("@Search", search.ToLower());

            var results = _db.Query("SELECT * FROM @TableName WHERE @Trigger = '@Search'", parms);
            if (results.Rows.Count == 0)
            {
                Match match = Regex.Match(search, @"([\w].*[a-zA-Z])");
                if (match.Success)
                {
                    //If it failed, check for the message with symbols stripped.
                    parms["@Search"] = match.Groups[1].Value;

                    results = _db.Query("SELECT * FROM @TableName WHERE @Trigger = '@Search'", parms);
                    if (results.Rows.Count == 0) return null;
                }
            }

            //If results found, convert to a list of messages.
            var list = new List<MessageObject>();
            foreach (DataRow r in results.Rows)
                list.Add(CreateObject(table, r));
            return list;
        }

        public List<MessageObject> GetDistinctByCol(TableType table, string columnName)
        {
            var type = GetTable(table);

            //Get the message to be edited
            var parms = new Dictionary<string, string>();
            parms.Add("@TableName", type.TableName);
            parms.Add("@ColName", columnName);

            var results = _db.Query("SELECT DISTINCT @ColName FROM @TableName", parms);
            if (results.Rows.Count == 0) return null;

            //If results found, convert to a list of messages.
            var list = new List<MessageObject>();
            foreach (DataRow r in results.Rows)
                list.Add(CreateObject(table, r));
            return list;
        }

        public MessageObject CreateObject(TableType table, DataRow result)
        {
            var type = GetTable(table);
            var message = type.Message(result);

            return new MessageObject(Irc.Self, message, result);
        }

        public MessageObject GetRandomBy(TableType table, string columnName, string search)
        {
            var searchBy = SearchBy(table, columnName, search);
            if (searchBy == null) return null;
            var list = searchBy.ToArray();
            if (list.Length == 0) return null;

            var random = new Random();
            int r = random.Next(0, list.Length);

            var obj = list[r];
            return obj;
        }
    }

    public abstract class Table
    {
        public abstract string Name { get; }
        public abstract string TableName { get; }
        public abstract string IdName { get; }
        public abstract string EditName { get; }

        public abstract string Message(DataRow result);
    }

    public class RepeatTable : Table
    {
        public override string Name { get { return "Repeat"; } }
        public override string TableName { get { return "repeats"; } }
        public override string IdName { get { return "repeatId"; } }
        public override string EditName { get { return "message"; } }

        public override string Message(DataRow result)
        {
            var reply = result["message"].ToString();
            return reply;
        }
    }

    public class SpecialTable : Table
    {
        public override string Name { get { return "Special"; } }
        public override string TableName { get { return "special_commands"; } }
        public override string IdName { get { return "specialId"; } }
        public override string EditName { get { return "reply"; } }

        public override string Message(DataRow result)
        {
            var reply = result["reply"].ToString();
            return reply;
        }
    }

    public class CommandTable : Table
    {
        public override string Name { get { return "Command"; } }
        public override string TableName { get { return "commands"; } }
        public override string IdName { get { return "commandId"; } }
        public override string EditName { get { return "reply"; } }

        public override string Message(DataRow result)
        {
            var reply = result["reply"].ToString();
            return reply;
        }
    }

    public class QuestionTable : Table
    {
        public override string Name { get { return "Question"; } }
        public override string TableName { get { return "questions"; } }
        public override string IdName { get { return "questionId"; } }
        public override string EditName { get { return "question"; } }

        public override string Message(DataRow result)
        {
            //Create the message for display
            var question = result["question"].ToString();
            return question;
        }
    }

    public class QuoteTable : Table
    {
        public override string Name { get { return "Quote"; } }
        public override string TableName { get { return "quotes"; } }
        public override string IdName { get { return "quoteId"; } }
        public override string EditName { get { return "quote"; } }

        public override string Message(DataRow result)
        {
            //Create the message for display
            var quote = result["quote"].ToString();
            var user = result["user"].ToString();
            user = char.ToUpper(user[0]) + user.Substring(1);
            var message = '"' + quote + '"' + " -" + user;
            return message;
        }
    }

    public class ReplyTable : Table
    {
        public override string Name { get { return "Reply"; } }
        public override string TableName { get { return "replies"; } }
        public override string IdName { get { return "replyId"; } }
        public override string EditName { get { return "reply"; } }

        public override string Message(DataRow result)
        {
            //Create the message for display
            var reply = result["reply"].ToString();
            return reply;
        }
    }

}
