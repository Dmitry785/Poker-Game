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
        public PokerTableViewModel PokerTableViewModel { get;}
        public int MaxBet
        {
            get => maxBet;
            set
            {
                maxBet = value;
                OnPropertyChanged();
            }
        }
        public int CurrentBet
        {
            get => currentBet;
            set
            {
                currentBet = value;
                OnPropertyChanged();
            }
        }
        public ICommand BetCommand { get; }
        public ICommand CallCommand { get; }
        public ICommand CheckCommand { get; }
        public ICommand RaiseCommand { get; }
        public ICommand FoldCommand { get; }
        public bool CanBet
        {
            get => canBet;
            set {
                canBet = value;
                OnPropertyChanged();
            }
        }
        public bool CanCall
        {
            get => canCall;
            set {
                canCall = value;
                OnPropertyChanged();
            }
        }
        public bool CanCheck
        {
            get => canCheck;
            set {
                canCheck = value;
                OnPropertyChanged();
            }
        }
        public bool CanRaise
        {
            get => canRaise;
            set {
                canRaise = value;
                OnPropertyChanged();
            }
        }
        public bool CanFold
        {
            get => canFold;
            set {
                canFold = value;
                OnPropertyChanged();
            }
        }
        public GameViewModel(GameService game, SignalBus sb)
        {
            BetCommand = new Command(OnBet);
            CallCommand = new Command(OnCall);
            CheckCommand = new Command(OnCheck);
            RaiseCommand = new Command(OnRaise);
            FoldCommand = new Command(OnFold);
            PokerTableViewModel = new PokerTableViewModel();
            _game = game;
        }
        private void OnBet()
        {
            MessageBox.Show("bet");
        }
        private void OnCall()
        {
            MessageBox.Show("call");
        }
        private void OnCheck()
        {
            MessageBox.Show("check");
        }
        private void OnRaise()
        {
            MessageBox.Show("raise");
        }
        private void OnFold()
        {
            MessageBox.Show("fold");
        }
        private void HandleGameChanged()
        {

        }
        private int currentBet = 200;
        private int maxBet = 500;
        private bool canBet = true;
        private bool canCall = true;
        private bool canCheck = true;
        private bool canRaise = true;
        private bool canFold = true;
    }
}
