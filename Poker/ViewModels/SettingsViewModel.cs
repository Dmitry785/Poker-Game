using Poker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public string CurrentState => _game.State.ToString();
        public string HostIP
        {
            get => hostIP;
            set
            {
                hostIP = value;
                OnPropertyChanged();
            }
        }
        public string CurrentIP => _game.CurrentAddress.ToString()!;
        public ICommand ConnectCommand{ get; }
        public ICommand StartHostCommand { get; }
        public ICommand DisconnectCommand { get; }
        public bool CanConnect => 
            _game.State is Connection.ConnectionState.NotConnected;
        public bool CanHost =>
            _game.State is Connection.ConnectionState.NotConnected;
        public bool CanDisconnect =>
            _game.State is not Connection.ConnectionState.NotConnected;
        public ObservableCollection<string> SettingsLog { get; set; } = new();
        public SettingsViewModel(GameService game, SignalBus sb)
        {
            _game = game;
            ConnectCommand = new Command(OnConnect);
            StartHostCommand = new Command(OnStartHost);
            DisconnectCommand = new Command(OnDisconnect);
            sb.Subscribe<StateChangedMessage>(HandleStatusChanged);
            /*NetworkChange.NetworkAddressChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(CurrentIP));
            };*/
        }
        private async void OnStartHost()
        {
            await _game.HandleLocalCommand(new StartHostCommand());
        }
        private async void OnConnect()
        {
            if (!IPEndPoint.TryParse(HostIP, out var ip))
                return;
            await _game.HandleLocalCommand(new ConnectCommand(ip));
        }
        private async void OnDisconnect()
        {
            await _game.HandleLocalCommand(new DisconnectCommand());
        }
        private void HandleStatusChanged(StateChangedMessage message)
        {
            OnPropertyChanged(nameof(CurrentState));
            OnPropertyChanged(nameof(CanConnect));
            OnPropertyChanged(nameof(CanHost));
            OnPropertyChanged(nameof(CanDisconnect));
        }
        private string hostIP = "localhost:7777";
    }
}
