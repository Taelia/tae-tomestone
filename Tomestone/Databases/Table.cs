using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using TomeLib.Db;
using Tomestone.Chatting;

namespace Tomestone.Databases
{
    public class Table
    {
        private readonly Database _database;
        private readonly string _tableName;
        private readonly IReplyType _messageType;

        public Table(string tableName, IReplyType messageType, Database database)
        {
            _database = database;
            _messageType = messageType;
            _tableName = tableName;
        }

        public bool Insert(string addedBy, string trigger, string reply)
        {
            var data = new Dictionary<string, string>();
            data.Add("addedBy", addedBy);
            data.Add("trigger", trigger);
            data.Add("command", reply);

            return _database.Insert(_tableName, data);
        }

        public bool Edit(string id, string toReplace, string replaceWith)
        {
            if (toReplace == replaceWith)
                return false;

            var entry = GetById(id);

            if (entry == null)
                return false;

            var newMessage = entry.Message.Replace(toReplace, replaceWith);

            var data = new Dictionary<string, string>();
            data.Add("reply", newMessage);

            var parms = new Dictionary<string, string>();
            parms.Add("@IdName", "id");
            parms.Add("@Id", id);

            var ok = _database.Update(_tableName, data, "@IdName = '@Id'", parms);
            return ok;
        }

        public bool Delete(string id)
        {
            var parms = new Dictionary<string, string>();
            parms.Add("@IdName", "id");
            parms.Add("@Id", id);

            var ok = _database.Delete(_tableName, "@IdName = '@Id'", parms);
            return ok;
        }

        public TableReply GetById(string id)
        {
            var parms = new Dictionary<string, string>();
            parms.Add("@TableName", _tableName);
            parms.Add("@IdName", "id");
            parms.Add("@Id", id);

            var results = _database.Query("SELECT * FROM @TableName WHERE @IdName = @Id", parms);
            if (results.Rows.Count == 0) return null;

            var result = results.Rows[0];
            return DataRowToMessage(result);
        }

        public TableReply GetLatestEntry()
        {
            var parms = new Dictionary<string, string>();
            parms.Add("@TableName", _tableName);
            parms.Add("@IdName", "id");

            var results = _database.Query("SELECT * FROM @TableName ORDER BY @IdName DESC", parms);
            if (results.Rows.Count == 0) return null;

            var result = results.Rows[0];
            return DataRowToMessage(result);
        }

        // returns all entries in a table based on a column type and a text field
        public List<TableReply> SearchBy(string columnName, string search)
        {
            //First check for the message as is.
            search = search.ToLower();

            var parms = new Dictionary<string, string>();
            parms.Add("@TableName", _tableName);
            parms.Add("@Trigger", columnName);
            parms.Add("@Search", search.ToLower());

            var results = _database.Query("SELECT * FROM @TableName WHERE @Trigger = '@Search'", parms);
            if (results.Rows.Count == 0)
            {
                Match match = Regex.Match(search, @"([\w].*[a-zA-Z])");
                if (match.Success)
                {
                    //If it failed, check for the message with symbols stripped.
                    parms["@Search"] = match.Groups[1].Value;

                    results = _database.Query("SELECT * FROM @TableName WHERE @Trigger = '@Search'", parms);
                    if (results.Rows.Count == 0) return null;
                }
            }

            var entries = new List<TableReply>();
            foreach (DataRow result in results.Rows)
                entries.Add(DataRowToMessage(result));

            return entries;
        }

        public TableReply GetRandomBy(string columnName, string search)
        {
            var list = SearchBy(columnName, search);
            if (list == null || list.Count == 0) return null;

            var random = new Random();
            int r = random.Next(0, list.Count);

            var obj = list[r];
            return obj;
        }

        private TableReply DataRowToMessage(DataRow dataRow)
        {
            var entry = new TableEntry(dataRow["id"].ToString(), dataRow["addedBy"].ToString(), dataRow["trigger"].ToString(), dataRow["reply"].ToString());
            var tomeMessage = new TableReply(_messageType, entry);
            return tomeMessage;
        }
    }
}
