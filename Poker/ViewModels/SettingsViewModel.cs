using Poker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Poker.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private GameService _game;
        private string hostIP = "127.0.0.1:7777";
        public string CurrentState => _game.CurrentState.ToString();
        public string HostIP
        {
            get => hostIP;
            set
            {
                HostIP = value;
                OnPropertyChanged();
            }
        }
        public ICommand ConnectCommand{ get; }
        public SettingsViewModel(GameService game, SignalBus sb)
        {
            _game = game;
            ConnectCommand = new Command(OnConnect);
            sb.Subscribe<StateChangedMessage>(HandleStatusChanged);
        }
        private void OnConnect()
        {
            if (!IPEndPoint.TryParse(HostIP, out var ip))
                return;
            _game.Connect(ip);
        }
        private void HandleStatusChanged(StateChangedMessage message)
        {
            OnPropertyChanged(nameof(CurrentState));
        }
    }
}
