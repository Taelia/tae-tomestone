using System.Text.RegularExpressions;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public abstract class BaseAdminCommand : ICommand
    {
        protected abstract string RegexString { get; }
        private readonly string _adminChannel;

        protected BaseAdminCommand(string adminChannel)
        {
            _adminChannel = adminChannel;
        }

        public bool Parse(UserMessage message)
        {
            Match match = Regex.Match(message.Message, RegexString);
            var isAdminChannel = message.Channel.Name == _adminChannel;

            return match.Success && isAdminChannel;
        }

        public abstract TomeReply Execute(UserMessage message);
    }
}