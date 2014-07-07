using Meebey.SmartIrc4net;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public interface ICommand
    {
        bool Parse(string message);
        string Execute(UserMessage message);
    }
}