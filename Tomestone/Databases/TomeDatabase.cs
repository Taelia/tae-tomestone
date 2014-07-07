/*** NOTES
 * future changes: 
 * - restructure the database interface to use 1 tabletype instead of having to create a new object structure for each new table
 *   that is added. Goal: get rid of hardcoding.
 *   Could implement a generic 'table' class that will work on all of the above, and return 'null' if the table happens to miss 
 *   one of these fields for said field.
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using TomeLib.Db;
using Tomestone.Chatting;
using Tomestone.Models;

namespace Tomestone.Databases
{
    public class ChatDatabase
    {
        public readonly List<TableReply> ReplyCache = new List<TableReply>();


        public readonly Dictionary<string, Table> Tables = new Dictionary<string, Table>();

        public readonly UserTable UserTable;

        private Database _database { get { return Main.Db; } }

        public ChatDatabase()
        {
            Tables.Add("repeat", new Table("repeats", new DefaultReply("Repeat"),  _database));
            Tables.Add("special", new Table("special_commands", new DefaultReply("Special command"), _database));
            Tables.Add("command", new Table("commands", new DefaultReply("Command"), _database));
            Tables.Add("reply", new Table("replies", new DefaultReply("Reply"), _database));
            Tables.Add("quote", new Table("quotes", new QuoteReply(), _database));

            UserTable = new UserTable(_database);
        }
    }
}
