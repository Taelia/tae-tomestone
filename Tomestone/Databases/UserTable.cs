using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomeLib.Db;

namespace Tomestone.Databases
{
    public class UserTable
    {
        private Database _database;
        private string _tableName = "users";

        public UserTable(Database database)
        {
            _database = database;
        }

        // An interface function implemented to obtain all unique values of the specified column
        public List<DataRow> GetDistinctByCol(string columnName)
        {
            //Get the message to be edited
            var parms = new Dictionary<string, string>();
            parms.Add("@TableName", _tableName);
            parms.Add("@ColName", columnName);

            var results = _database.Query("SELECT DISTINCT @ColName FROM @TableName", parms);
            if (results.Rows.Count == 0) return null;

            return results.Select().ToList();
        }
    }
}