﻿using Tomestone.Chatting;
using Tomestone.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TomeLib.Db;
using TomeLib.Irc;

namespace Tomestone.Models
{
    public class Main
    {
        public MainViewModel View;


        public TomeChat Chat;

        private Database Db { get { return Database.GetDatabase("tomestone.db"); } }

        //private string botName = "Tomestone";
        //private string botOauth = "oauth:e1dmmpdsq34uppmhxdbqmhbesja6osr";


        public static string chatMain = "#taelia_";
        public static string chatMods = "#taelia_mods";

        //This is the first model to be created.
        //From here, start the different parts of the screen.
        public Main(MainViewModel view)
        {
            View = view;

            InitializeDatabase();

            FillLoginInfo();
        }

        private void InitializeDatabase()
        {
            Db.SafeQuery("CREATE TABLE IF NOT EXISTS login ( loginId INTEGER PRIMARY KEY AUTOINCREMENT, name, oauth, main, mods, remember NOT NULL );");
            Db.SafeQuery("CREATE TABLE IF NOT EXISTS commands (commandId INTEGER PRIMARY KEY AUTOINCREMENT, user, command, reply, UNIQUE ( command, reply ) );");
            Db.SafeQuery("CREATE TABLE IF NOT EXISTS quotes ( quoteId INTEGER PRIMARY KEY AUTOINCREMENT, user NOT NULL, quote NOT NULL );");
            Db.SafeQuery("CREATE TABLE IF NOT EXISTS repeats ( repeatId INTEGER PRIMARY KEY AUTOINCREMENT, user NOT NULL, time NOT NULL, message NOT NULL);");
            Db.SafeQuery("CREATE TABLE IF NOT EXISTS replies ( replyId INTEGER PRIMARY KEY AUTOINCREMENT, user NOT NULL, [trigger] NOT NULL, reply NOT NULL, CONSTRAINT 'uc_message' UNIQUE ( [trigger], reply ));");
            Db.SafeQuery("CREATE TABLE IF NOT EXISTS special_commands ( specialId INTEGER PRIMARY KEY AUTOINCREMENT, user NOT NULL, command NOT NULL, reply NOT NULL );");
            // user database for tracking optout of quotes
            Db.SafeQuery("CREATE TABLE IF NOT EXISTS users ( userId INTEGER PRIMARY KEY AUTOINCREMENT, user NOT NULL, optOut NOT NULL );");

            var results = Db.Query("SELECT * FROM login", new Dictionary<string, string>());

            if (results.Rows.Count == 0)
            {
                var data = new Dictionary<string, string>();
                data.Add("name", "");
                data.Add("oauth", "");
                data.Add("main", "");
                data.Add("mods", "");
                data.Add("remember", "false");
                Db.Insert("login", data);
            }
        }

        private void FillLoginInfo()
        {
            var results = Db.Query("SELECT * FROM login", new Dictionary<string, string>());

            //If there is an entry in the login table, use it to fill the info
            var result = results.Rows[0];
            View.Login = result["name"].ToString();
            View.OAuth = result["oauth"].ToString();
            View.Main = result["main"].ToString();
            View.Mods = result["mods"].ToString();
            View.Remember = result["remember"].ToString() == "true";
        }

        public void Start()
        {
            RememberCredentials();

            chatMain = View.Main;
            chatMods = View.Mods;

            Chat = new TomeChat(View.Login, View.OAuth, View.Main, View.Mods);
        }

        private void RememberCredentials()
        {
            if (View.Remember)
            {
                var data = new Dictionary<string, string>();
                data.Add("name", View.Login);
                data.Add("oauth", View.OAuth);
                data.Add("main", View.Main);
                data.Add("mods", View.Mods);
                data.Add("remember", "true");

                Db.Update("login", data, "loginId = '1'", new Dictionary<string, string>());
            }
            else
            {
                var data = new Dictionary<string, string>();
                data.Add("name", "");
                data.Add("oauth", "");
                data.Add("main", "");
                data.Add("mods", "");
                data.Add("remember", "false");
                Db.Update("login", data, "loginId = '1'", new Dictionary<string, string>());
            }
        }


    }
}
