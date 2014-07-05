using System;
using System.Collections.Generic;
using System.Linq;
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
using Tomestone.Models;

namespace Tomestone.Chatting
{
    public partial class TomeChat : ChatBase
    {
        private ParseCommands _parse;

        public History SentMessages { get; set; }
        public History ReceivedMessages { get; set; }

        private TwitchConnection _twitch;
        private Database _db { get { return Main.Db; } }

        public ChatDatabase Chat;

        private DispatcherTimer _timer;

        public static IrcUser Self;

        public TomeChat(string login, string pass, string main, string mods)
            : base(new Irc(login, pass, new[] { main, mods }))
        {
            Self = Client.GetIrcUser(login);

            SentMessages = new History();
            ReceivedMessages = new History();

            _twitch = new TwitchConnection();

            Chat = new ChatDatabase(_db);

            _parse = new ParseCommands(this, Chat);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMinutes(1);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        void _timer_Tick(object sender, EventArgs e)
        {
            _parse.ParseRepeat();
        }


        protected override void OnMessage(Channel channel, IrcUser from, string message)
        {
            try
            {
                var obj = new ChatMessage(from, message);

                //Check against first word how to handle the message
                var command = message.TrimStart(' ').Split(' ')[0];
                switch (command[0])
                {
                    case '@':
                        AdminCommands(channel, from, message, command);
                        break;
                    case '!':
                        UserCommands(channel, from, message, command);
                        break;
                    default:
                        DefaultCommands(channel, from, message, command);
                        break;
                }
            }
            catch (Exception e)
            {
                var x = e;
                { }
            }
        }

        protected override void OnAction(Channel channel, IrcUser from, string message)
        {
            //Treat the same as messages.
            OnMessage(channel, from, message);
        }

        protected override void OnJoin(Channel channel, string from)
        {
        }

        //Part and Quit don't work properly on Twitch
        protected override void OnPart(Channel channel, string from)
        {
        }

        protected override void OnQuit(Channel channel, string from)
        {
            //Treat the same as parts.
            OnPart(channel, from);
        }
    }
}
