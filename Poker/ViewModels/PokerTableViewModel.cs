using Poker.Models;
using Poker.Services;
using Poker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Poker.ViewModels
{
    public class PokerTableViewModel : BaseViewModel
    {
        private GameService _game;
        public PlayerViewModel? Player1
        {
            get => player1;
            set
            {
                player1 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player1Visibility));
            }
        }
        public PlayerViewModel? Player2
        {
            get => player2;
            set
            {
                player2 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player2Visibility));
            }
        }
        public PlayerViewModel? Player3
        {
            get => player3;
            set
            {
                player3 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player3Visibility));
            }
        }
        public PlayerViewModel? Player4
        {
            get => player4;
            set
            {
                player4 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player4Visibility));
            }
        }
        public PlayerViewModel? Player5
        {
            get => player5;
            set
            {
                player5 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player5Visibility));
            }
        }
        public PlayerViewModel? Player6
        {
            get => player6;
            set
            {
                player6 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player6Visibility));
            }
        }
        public Visibility Player1Visibility => (Player1 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Player2Visibility => (Player2 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Player3Visibility => (Player3 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Player4Visibility => (Player4 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Player5Visibility => (Player5 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Player6Visibility => (Player6 is null) ? Visibility.Hidden : Visibility.Visible;
        public void SetPlayer(PlayerInfo player)
        {
            switch (player.SeatIndex)
            {
                case 0:
                    Player1 = new PlayerViewModel(player);
                    break;
                case 1:
                    Player2 = new PlayerViewModel(player);
                    break;
                case 2:
                    Player3 = new PlayerViewModel(player);
                    break;
                case 3:
                    Player4 = new PlayerViewModel(player);
                    break;
                case 4:
                    Player5 = new PlayerViewModel(player);
                    break;
                case 5:
                    Player6 = new PlayerViewModel(player);
                    break;
            }
        }
        public void ClearPlayers()
        {
            Player1 = null;
            Player2 = null;
            Player3 = null;
            Player4 = null;
            Player5 = null;
            Player6 = null;
        }
        public PokerTableViewModel()
        {
            Player1 = new PlayerViewModel(new Models.PlayerInfo("Dmitry", 1000, 0));
            Player3 = new PlayerViewModel(new Models.PlayerInfo("Oleg", 2000, 0));
            Player3.CurrentMove = "Fold";
            Player4 = new PlayerViewModel(new Models.PlayerInfo("Vlad", 1500, 0));
        }
        private PlayerViewModel? player1;
        private PlayerViewModel? player2;
        private PlayerViewModel? player3;
        private PlayerViewModel? player4;
        private PlayerViewModel? player5;
        private PlayerViewModel? player6;
    }
}
