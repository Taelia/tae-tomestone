using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    // times out the specified user from using tome commands for a specified time
    // assumes that the username is provided as the first argument after check
    // also assumes that the second parameter is a numerical input
    public class AdminTomeoutCommand : ICommand
    {
        private readonly TomeChat _chat;
        private Dictionary<string, DateTime> _blacklist;

        private const string RegexString = "@tomeout (.+?) ([0-9]+)";

        public AdminTomeoutCommand(TomeChat chat)
        {
            _blacklist = new Dictionary<string, DateTime>();
            _chat = chat;
        }

        
        public bool Parse(string message)
        {
            // separate the text into the command to be queried, and additional parameters (if applicable)
            Match match = Regex.Match(message, RegexString);
            return match.Success;             
        }

        public string Execute(UserMessage userMessage)
        {
            if (userMessage.Channel != _chat.ModChannel)
                return "";

            Match match = Regex.Match(userMessage.Message, RegexString);

            if (match.Success)
            {
                string username = match.Groups[1].Value;
                DateTime duration = DateTime.Now + TimeSpan.FromMinutes(Double.Parse(match.Groups[2].Value));

                // only process the command if the user isnt already timed out
                if (!_blacklist.ContainsKey(username)) _blacklist.Add(username, duration);
            }

            //_chat.SendStatus(Main.chatMods, "SYNTAX: '@tomeout A B' where A is a username, and B is a duration in minutes.");

            return "";
        }

        public Boolean CheckUserBlacklist(string username)
        {
            // check if the specified username is in the blacklist.
            if (_blacklist.ContainsKey(username))
            {
                // now check if the timeout has expired
                if (_blacklist[username].CompareTo(DateTime.Now) < 0)
                {
                    // remove the user from the timeout list and return false (not timed out)
                    _blacklist.Remove(username);
                    return false;
                }
                // still timed out
                return true;
            }
            return false;
        }

    
    }
}
