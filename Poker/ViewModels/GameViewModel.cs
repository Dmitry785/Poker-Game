using Poker.Models;
using Poker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Poker.ViewModels
{
    public class GameViewModel : BaseViewModel
    {
        private GameService _game;
        public PokerTableViewModel PokerTableViewModel { get; }
        public ICommand CallCommand { get; }
        public ICommand CheckCommand { get; }
        public ICommand BetRaiseCommand { get; }
        public ICommand FoldCommand { get; }
        public ICommand StartGameCommand { get; }
        public decimal CurrentBet
        {
            get => currentBet;
            set
            {
                currentBet = value;
                OnPropertyChanged();
            }
        }
        public decimal MaxBetRaise=> _game.LocalPlayer?.Money ?? 0;
        public decimal MinBetRaise => _game.MinBetRaise;
        public bool IsMyTurn => _game.CurrentPlayerIndex == 0 && IsGameStarted;
        public bool IsGameStarted => _game.CurrentGameStage != GameStage.None;
        public decimal AmountToCall {
            get
            {
                var localPlayer = _game.LocalPlayer;
                if (localPlayer is null)
                    return 0;
                return _game.CurrentMaxBet - localPlayer.CurrentBet;
            }
        }
        public bool CanStartGame =>
            _game.State is Connection.ConnectionState.Hosting &&
            !IsGameStarted &&
            _game.Players.Count > 1;
        public bool CanFold => IsMyTurn;
        public bool CanCheck => IsGameStarted && AmountToCall == 0 && IsMyTurn;
        public bool CanCall
        {
            get
            {
                var localPlayer = _game.LocalPlayer;
                if (localPlayer is null)
                    return false;
                return IsMyTurn && AmountToCall > 0 && localPlayer.Money > AmountToCall;
            }
        }
        public bool CanBetRaise
        {
            get
            {
                var localPlayer = _game.LocalPlayer;
                if (localPlayer is null)
                    return false;
                return IsMyTurn && localPlayer.Money > AmountToCall;
            }
        }
        public bool CanAllIn
        {
            get
            {
                var localPlayer = _game.LocalPlayer;
                if (localPlayer is null)
                    return false;
                return IsMyTurn && localPlayer.Money > 0;
            }
        }
        public decimal Pot => _game.Pot;
        public GameViewModel(GameService game, SignalBus sb)
        {
            BetRaiseCommand = new Command(OnBetRaise);
            CallCommand = new Command(OnCall);
            CheckCommand = new Command(OnCheck);
            FoldCommand = new Command(OnFold);
            StartGameCommand = new Command(OnStartGame);
            PokerTableViewModel = new PokerTableViewModel();
            sb.Subscribe<PlayerUpdatedMessage>(HandlePlayerUpdatedMessage);
            sb.Subscribe<TableStateChangedMessage>(HandleTableStateChangedMessage);
            sb.Subscribe<PlayerLeftMessage>(HandlePlayerLeftMessage);
            _game = game;
        }
        private async void OnStartGame()
        {
            await _game.HandleLocalCommand(new StartGameCommand());
        }
        private async void OnBetRaise()
        {
            await _game.HandleLocalCommand(new BetRaiseCommand(CurrentBet));
        }
        private async void OnCall()
        {
            await _game.HandleLocalCommand(new CallCommand());
        }
        private async void OnCheck()
        {
            await _game.HandleLocalCommand(new CheckCommand());
        }
        private async void OnFold()
        {
            await _game.HandleLocalCommand(new FoldCommand());
        }
        private void OnGameChanged()
        {
            OnPropertyChanged(nameof(IsMyTurn));
            OnPropertyChanged(nameof(CanStartGame));
            OnPropertyChanged(nameof(CanFold));
            OnPropertyChanged(nameof(CanCheck));
            OnPropertyChanged(nameof(CanCall));
            OnPropertyChanged(nameof(CanBetRaise));
            OnPropertyChanged(nameof(CanAllIn));
            OnPropertyChanged(nameof(Pot));
            OnPropertyChanged(nameof(MaxBetRaise));
            OnPropertyChanged(nameof(MinBetRaise));
        }
        private void HandlePlayerLeftMessage(PlayerLeftMessage message)
        {

        }
        private void HandlePlayerUpdatedMessage(PlayerUpdatedMessage message)
        {
            PokerTableViewModel.UpsertPlayer(message.player, message.currentMove);
            OnGameChanged();
        }
        private void HandleTableStateChangedMessage(TableStateChangedMessage message)
        {
            PokerTableViewModel.UpdateCommunityCards(message.communityCards);
            OnGameChanged();
        }
        private decimal currentBet = 0;
    }
}
