using System.Security.Cryptography.X509Certificates;
using Meebey.SmartIrc4net;
using Tomestone.Chatting;

namespace Tomestone.Commands
{
    public interface ICommand
    {
        bool Parse(UserMessage message);
        void Execute(UserMessage message);
    }
}