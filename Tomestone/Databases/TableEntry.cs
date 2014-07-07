using System.Collections.Generic;
using System.Data;
using TomeLib.Db;

namespace Tomestone.Databases
{
    public class TableEntry
    {
        public string Id { get; private set; }
        public string AddedBy { get; private set; }
        public string Trigger { get; private set; }
        public string Reply { get; private set; }

        public TableEntry(string id, string addedBy, string trigger, string reply)
        {
            Id = id;
            AddedBy = addedBy;
            Trigger = trigger;
            Reply = reply;
        }
    }
}