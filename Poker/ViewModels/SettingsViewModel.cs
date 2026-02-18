using Poker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        public string CurrentIP => _game.CurrentIP.ToString();
        public ICommand ConnectCommand{ get; }
        public ICommand StartHostCommand { get; }
        public SettingsViewModel(GameService game, SignalBus sb)
        {
            _game = game;
            ConnectCommand = new Command(OnConnect);
            StartHostCommand = new Command(OnStartHost);
            sb.Subscribe<StateChangedMessage>(HandleStatusChanged);
            NetworkChange.NetworkAddressChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(CurrentIP));
            };
        }
        private void OnStartHost()
        {
            MessageBox.Show("Start host");
        }
        private void OnConnect()
        {
            if (!IPEndPoint.TryParse(HostIP, out var ip))
                return;
            MessageBox.Show(HostIP);
            _game.Connect(ip);
        }
        private void HandleStatusChanged(StateChangedMessage message)
        {
            OnPropertyChanged(nameof(CurrentState));
        }
    }
}
