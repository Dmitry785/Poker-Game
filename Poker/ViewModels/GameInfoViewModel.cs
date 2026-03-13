using Poker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Poker.ViewModels
{
    public class GameInfoViewModel : BaseViewModel
    {
        public ObservableCollection<string> GameLog { get; set; } = new();
        public ObservableCollection<string> ChatLog { get; set; } = new();
        public Command SendMessageToChat { get; }
        public string ChatInputText
        {
            get => chatInputText;
            set
            {
                chatInputText = value;
                OnPropertyChanged();
            }
        }
        public string PlayerName
        {
            get => _game.ClientName;
            set
            {
                _game.ClientName = value;
                OnPropertyChanged();
            }
        }
        public bool IsConnectedOrHosting =>
            _game.State is Connection.ConnectionState.Connected or Connection.ConnectionState.Hosting;
        private GameService _game;
        public GameInfoViewModel(GameService game, SignalBus sb)
        {
            _game = game;
            SendMessageToChat = new Command(OnSendMessage);
            sb.Subscribe<StateChangedMessage>(OnStateChangedMessage);
            sb.Subscribe<ChatMessageReceivedMessage>(OnChatMessageReceived);
        }
        private void OnChatMessageReceived(ChatMessageReceivedMessage message)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                ChatLog.Add($"{message.senderName} >> {message.message}");
            });
        }
        private async void OnSendMessage()
        {
            if (ChatInputText == string.Empty)
                return;
            await _game.HandleLocalCommand(new SendCommonMessage(ChatInputText));
            ChatInputText = string.Empty;
        }
        private void OnStateChangedMessage(StateChangedMessage message)
        {
            OnPropertyChanged(nameof(IsConnectedOrHosting));
        }
        private string chatInputText = string.Empty;
    }
}
