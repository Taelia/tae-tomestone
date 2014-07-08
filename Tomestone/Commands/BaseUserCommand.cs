using System.Text.RegularExpressions;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public abstract class BaseUserCommand : ICommand
    {
        protected abstract string RegexString { get; }

        public bool Parse(UserMessage userMessage)
        {
            Match match = Regex.Match(userMessage.Message, RegexString);
            return match.Success;
        }

        public abstract TomeReply Execute(UserMessage message);
    }
}