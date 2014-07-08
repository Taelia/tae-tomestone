using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tomestone.Commands;
using Tomestone.Chatting;
using System.Windows;
using TomeLib.Irc;
using TomeLib.Db;
using Meebey.SmartIrc4net;
using TomeLib.Twitch;
using System.Windows.Threading;
using Tomestone.Databases;
using Tomestone.Models;

namespace Tomestone.Chatting
{
    public partial class TomeChat : ChatBase
    {
        private readonly ParseCommands _parse;

        public readonly History<UserMessage> ReceivedMessages = new History<UserMessage>();

        private TwitchConnection _twitch;

        public readonly Dictionary<string, string> Channels = new Dictionary<string, string>();

        private readonly List<ICommand> _commands = new List<ICommand>();

        public TomeChat(string login, string pass, string main, string mods)
            : base(new Irc(login, pass, new[] { main, mods }))
        {
            Channels.Add("main", main);
            Channels.Add("mods", mods);

            _twitch = new TwitchConnection();

            var database = new ChatDatabase();

            _parse = new ParseCommands(this, database);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            timer.Tick += _timer_Tick;
            timer.Start();

            _commands.Add(new AdminAddCommand(database, Channels["mods"]));
            _commands.Add(new AdminDeleteCommand(database, Channels["mods"]));
            _commands.Add(new AdminEditCommand(database, Channels["mods"]));
            _commands.Add(new AdminEntryCommand(database, Channels["mods"]));
            _commands.Add(new AdminInfoCommand(database, Channels["mods"]));
            _commands.Add(new AdminRepeatCommand(database, Channels["mods"]));

            _commands.Add(new SuperQuoteCommand(database, Channels["main"].Substring(1)));

            _commands.Add(new UserHighlightCommand(this));
            _commands.Add(new UserQuoteCommand(database, ReceivedMessages));
            _commands.Add(new UserTeachCommand(database));

            _commands.Add(new SuperTeachReply(database));
            _commands.Add(new UserAddReply(database));
            _commands.Add(new UserTeachReply(database));
            _commands.Add(new UserQuoteReply(database));
        }

        void _timer_Tick(object sender, EventArgs e)
        {
        }


        protected override void OnMessage(Channel channel, IrcUser from, string message)
        {
            var userMessage = new UserMessage(channel, from, message);

            //TODO: If user is either opted out or blacklisted, return void here.

            //If a user message can be parsed into a command, print a reply message and ignore everything else.
            foreach (var command in _commands)
            {
                if (command.Parse(userMessage))
                {
                    try
                    {
                        ExecuteCommand(command, userMessage);
                    }
                    catch (Exception ex)
                    {
                        var x = ex;
                    }
                    return;
                }
            }
        }

        private void ExecuteCommand(ICommand command, UserMessage userMessage)
        {
            TomeReply reply = command.Execute(userMessage);
            SendMessage(reply.Channel.Name, reply.Message);
        }

        protected override void OnAction(Channel channel, IrcUser from, string message)
        {
            //Treat the same as messages.
            OnMessage(channel, from, message);
        }





        //We're not using joins and parts
        protected override void OnJoin(Channel channel, string from) { }
        protected override void OnPart(Channel channel, string from) { }
        protected override void OnQuit(Channel channel, string from) { }
    }
}
