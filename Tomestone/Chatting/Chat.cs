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

        private readonly string _mainChannel;
        public Channel MainChannel { get { return Client.GetChannel(_mainChannel); } }

        private readonly string _modChannel;
        public Channel ModChannel { get { return Client.GetChannel(_modChannel); } }

        private readonly List<ICommand> Commands = new List<ICommand>();

        public TomeChat(string login, string pass, string main, string mods)
            : base(new Irc(login, pass, new[] { main, mods }))
        {
            _mainChannel = main;
            _modChannel = mods;

            _twitch = new TwitchConnection();

            var database = new ChatDatabase();

            _parse = new ParseCommands(this, database);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            timer.Tick += _timer_Tick;
            timer.Start();

            Commands.Add(new AdminAddCommand(database, this));
            Commands.Add(new AdminDeleteCommand(database, this));
            Commands.Add(new AdminEditCommand(database, this));
            Commands.Add(new AdminEntryCommand(database, this));
            Commands.Add(new AdminInfoCommand(database, this));
            Commands.Add(new AdminRepeatCommand(database, this));

            Commands.Add(new SuperQuoteCommand(database, main.Substring(1)));

            Commands.Add(new UserAddCommand(database));
            Commands.Add(new UserHighlightCommand(this));
            Commands.Add(new UserQuoteCommand(database, ReceivedMessages));
            Commands.Add(new UserTeachCommand(database));
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            _parse.ParseRepeat();
        }


        protected override void OnMessage(Channel channel, IrcUser from, string message)
        {
            //TODO: If user is either opted out or blacklisted, return void here.

            //TODO: Execute returns a result string. Print it.
            foreach (var command in Commands)
                if (command.Parse(message)) command.Execute(new UserMessage(channel, from, message));

            //TODO:Below is depricated and should remove ASAP.
            //TODO:Revamp 'defaultCommands'

            //Check against first word how to handle the message
            var commandString = message.TrimStart(' ').Split(' ')[0];
            switch (commandString[0])
            {
                case '@':
                    _parse.ParseAdminCommands(channel, from, message, commandString);
                    break;
                case '!':
                    _parse.ParseUserCommands(channel, from, message, commandString);
                    break;
                default:
                    _parse.ParseDefaultCommands(channel, from, message, commandString);
                    ReceivedMessages.Add(new UserMessage(channel, from, message));
                    break;
            }
        }

        protected override void OnAction(Channel channel, IrcUser from, string message)
        {
            //Treat the same as messages.
            OnMessage(channel, from, message);
        }





        //We're not using joins and parts
        protected override void OnJoin(Channel channel, string from){}
        protected override void OnPart(Channel channel, string from){}
        protected override void OnQuit(Channel channel, string from){}
    }
}
