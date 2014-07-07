using System.Collections.Generic;
using System.Text.RegularExpressions;
using TomeLib.Irc;

namespace Tomestone.Chatting
{
    public class History<T> where T : IMessage
    {
        private readonly List<T> _history = new List<T>();

        public void Add(T obj)
        {
            _history.Add(obj);
        }

        public void Remove(T obj)
        {
            _history.Remove(obj);
        }

        public T Search(string search)
        {
            var array = _history.ToArray();

            //Search the array in reverse order
            for (var i = array.Length - 1; i >= 0; i--)
            {
                var message = array[i].Message;
                if (message.Contains(search))
                    return array[i];
            }
            return default(T);
        }

        public T Search(string user, string search)
        {
            var array = _history.ToArray();

            //Browse the array in reverse to search for the user and word.
            for (var i = array.Length - 1; i >= 0; i--)
            {
                var obj = array[i];
                var msg = obj.Message;
                var from = obj.From.Nick;

                //Check if the combination of nickname and word can be found in a message.
                var lookupNick = Regex.IsMatch(from, "^" + user + "$", RegexOptions.IgnoreCase);
                var lookupWord = Regex.IsMatch(msg, "\\b" + search + "\\b", RegexOptions.IgnoreCase);

                if (lookupNick && lookupWord)
                    return obj;
            }

            return default(T);
        }
    }
}