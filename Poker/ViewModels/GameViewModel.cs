using Poker.Models;
using Poker.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ILogger Logger = new NullLogger();
        public ICommand CallCommand { get; }
        public ICommand CheckCommand { get; }
        public ICommand BetRaiseCommand { get; }
        public ICommand FoldCommand { get; }
        public ICommand StartGameCommand { get; }
        public int CurrentBet
        {
            get => currentBet;
            set
            {
                currentBet = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPotentialBet));
            }
        }
        public decimal MyBalance => _game.LocalPlayer?.Money ?? 0;
        public decimal MaxBetRaise=> MyBalance;
        public decimal MinBetRaise => _game.MinBetRaise;
        public bool IsMyTurn => _game.LocalPlayer?.SeatIndex == _game.CurrentPlayerIndex && IsGameStarted;
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
        public bool CanCall => 
            IsMyTurn && AmountToCall > 0;
        public bool CanBetRaise => 
            IsMyTurn && MyBalance > AmountToCall &&
            _game.MinBetRaise > 0;
        public bool CanAllIn => IsMyTurn && MyBalance > 0;
        public decimal Pot => _game.Pot;
        public string GameStatusText => _game.CurrentGameStage.ToString();
        public decimal TotalPotentialBet => AmountToCall + CurrentBet;
        public GameViewModel(GameService game, SignalBus sb)
        {
            _game = game;
            BetRaiseCommand = new Command(OnBetRaise);
            CallCommand = new Command(OnCall);
            CheckCommand = new Command(OnCheck);
            FoldCommand = new Command(OnFold);
            StartGameCommand = new Command(OnStartGame);
            PokerTableViewModel = new PokerTableViewModel();
            sb.Subscribe<PlayersUpdatedMessage>(HandlePlayersUpdatedMessage);
            sb.Subscribe<PlayerTurnMessage>(HandlePlayerTurnMessage);
            sb.Subscribe<TableStateChangedMessage>(HandleTableStateChangedMessage);
            sb.Subscribe<PrivateCardDealtMessage>(HandlePrivateCardDealtMessage);
            sb.Subscribe<GameResultsOccurredMessage>(HandleGameResultsOccurredMessage);
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
        private void RefreshAll()
        {
            OnPropertyChanged(nameof(CanStartGame));
            OnPropertyChanged(nameof(CanFold));
            OnPropertyChanged(nameof(CanCheck));
            OnPropertyChanged(nameof(CanCall));
            OnPropertyChanged(nameof(CanBetRaise));
            OnPropertyChanged(nameof(CanAllIn));
            OnPropertyChanged(nameof(Pot));
            OnPropertyChanged(nameof(MaxBetRaise));
            OnPropertyChanged(nameof(MinBetRaise));
            OnPropertyChanged(nameof(GameStatusText));
        }
        private void HandlePrivateCardDealtMessage(PrivateCardDealtMessage message)
        {
            PokerTableViewModel.SetHand(message.hand);
        }
        private void HandlePlayersUpdatedMessage(PlayersUpdatedMessage message)
        {
            PokerTableViewModel.UpdatePlayers(message.players);
            RefreshAll();
        }
        private void HandlePlayerTurnMessage(PlayerTurnMessage message)
        {
            PokerTableViewModel.SetCurrentPlayerIndex(message.currentPlayerIndex);
            RefreshAll();
        }
        private void HandleGameResultsOccurredMessage(GameResultsOccurredMessage message)
        {
            RefreshAll();
        }
        private void HandleTableStateChangedMessage(TableStateChangedMessage message)
        {
            PokerTableViewModel.UpdateCommunityCards(message.communityCards);
            PokerTableViewModel.SetDealerIndex(message.dealerIndex);
            PokerTableViewModel.Pot = message.pot;
            RefreshAll();
        }
        private int currentBet = 0;
    }
}
