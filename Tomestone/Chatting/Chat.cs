using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using Tomestone.Commands;
using TomeLib.Irc;
using Meebey.SmartIrc4net;
using TomeLib.Twitch;
using System.Windows.Threading;

namespace Tomestone.Chatting
{
    public partial class TomeChat : ChatBase
    {
        public readonly History<UserMessage> ReceivedMessages = new History<UserMessage>();
        public readonly List<TomeReply> ReplyCache = new List<TomeReply>(); 

        private TwitchConnection _twitch;

        public readonly Dictionary<string, string> Channels = new Dictionary<string, string>();

        private readonly List<ICommand> _commands = new List<ICommand>();

        public TomeChat(string login, string pass, string main, string mods )
            : base(new Irc(login, pass, new[] { main, mods, "#tomestone"}))
        {
            Client.WriteLine("TWITCHCLIENT 3");

            Channels.Add("main", main);
            Channels.Add("mods", mods);
            Channels.Add("tome", "#tomestone");

            _twitch = new TwitchConnection();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
            timer.Tick += _timer_Tick;
            timer.Start();

            _commands.Add(new AdminAddCommand(this));
            _commands.Add(new AdminDeleteCommand(this));
            _commands.Add(new AdminEditCommand(this));
            _commands.Add(new AdminEntryCommand(this));
            _commands.Add(new AdminInfoCommand(this));

            _commands.Add(new SuperQuoteCommand(this));

            _commands.Add(new UserHighlightCommand(this));
            _commands.Add(new UserBribeCommand(this));
            _commands.Add(new UserTeachCommand(this));
            _commands.Add(new UserQuoteCommand(this));
            _commands.Add(new UserVoteCommand(this));

            _commands.Add(new SuperTeachReply(this));

            _commands.Add(new UserTeachReply(this));
            _commands.Add(new UserQuoteReply(this));
        }

        

        void _timer_Tick(object sender, EventArgs e)
        {
        }


        protected override void OnMessage(TwitchIrcChannel channel, TwitchChannelUser from, string message)
        {

            var userMessage = new UserMessage(channel, from, message);
            ReceivedMessages.Add(userMessage);
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
            command.Execute(userMessage);
        }

        protected override void OnAction(TwitchIrcChannel channel, TwitchChannelUser from, string message)
        {
            //Treat the same as messages.
            OnMessage(channel, from, message);
        }

        protected override void OnClear(TwitchIrcChannel channel, TwitchChannelUser from = null)
        {
            //
        }

        protected override void OnModeChange(TwitchIrcChannel channel, ChannelModes mode)
        {
            //
        }
    }
}
