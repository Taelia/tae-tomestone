﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tomestone.Chatting
{
    public class History
    {
        private readonly List<ChatMessage> _history = new List<ChatMessage>();

        public void Add(ChatMessage obj)
        {
            _history.Add(obj);
        }

        public void Remove(ChatMessage obj)
        {
            _history.Remove(obj);
        }

        public ChatMessage Search(string search)
        {
            var array = _history.ToArray();

            //Search the array in reverse order
            for (var i = array.Length - 1; i >= 0; i--)
            {
                var message = array[i].Message;
                if (message.Contains(search))
                    return array[i];
            }
            return null;
        }

        public ChatMessage Search(string user, string search)
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

            return null;
        }
    }
}