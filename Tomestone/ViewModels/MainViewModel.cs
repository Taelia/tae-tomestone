using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Caliburn.Micro;
using Tomestone.Models;
using System.Windows.Threading;

namespace Tomestone.ViewModels
{
    public class MainViewModel : PropertyChangedBase
    {
        private Main _model;

        //This is the start of the application.
        public MainViewModel()
        {
            _model = new Main(this);
        }

        private string _login;
        public string Login { get { return _login; } set { _login = value; NotifyOfPropertyChange("Login"); } }

        private string _oAuth;
        public string OAuth { get { return _oAuth; } set { _oAuth = value; NotifyOfPropertyChange("OAuth"); } }

        private string _main;
        public string Main { get { return _main; } set { _main = value; NotifyOfPropertyChange("Main"); } }

        private string _mods;
        public string Mods { get { return _mods; } set { _mods = value; NotifyOfPropertyChange("Mods"); } }

        private bool _remember;
        public bool Remember { get { return _remember; } set { _remember = value; NotifyOfPropertyChange("Remember"); } }

        public void Start()
        {
            _model.Start();
        }
    }
}
