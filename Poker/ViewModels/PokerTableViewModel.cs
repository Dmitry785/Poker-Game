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
        private readonly PlayerViewModel?[] _playerFields = new PlayerViewModel[6];
        public BordViewModel BordViewModel { get; set; }
        public PlayerViewModel? Player1
        {
            get => _playerFields[0];
            set
            {
                _playerFields[0] = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player1Visibility));
            }
        }
        public PlayerViewModel? Player2
        {
            get => _playerFields[1];
            set
            {
                _playerFields[1] = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player2Visibility));
            }
        }
        public PlayerViewModel? Player3
        {
            get => _playerFields[2];
            set
            {
                _playerFields[2] = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player3Visibility));
            }
        }
        public PlayerViewModel? Player4
        {
            get => _playerFields[3];
            set
            {
                _playerFields[3] = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player4Visibility));
            }
        }
        public PlayerViewModel? Player5
        {
            get => _playerFields[4];
            set
            {
                _playerFields[4] = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Player5Visibility));
            }
        }
        public PlayerViewModel? Player6
        {
            get => _playerFields[5];
            set
            {
                _playerFields[5] = value;
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
        public decimal Pot
        {
            get => pot;
            set
            {
                pot = value;
                OnPropertyChanged();
            }
        }
        public void UpdateCommunityCards(List<PokerCard> cards)
        {
            BordViewModel.UpdateCards(cards);
        }
        public void UpdatePlayers(List<PlayerInfo> players, 
            int currentPlayerIndex,
            int dealerIndex)
        {
            PlayerInfo? player = null;
            for (int i=0;i<6; i++)
            {
                player = players.FirstOrDefault(x => x.SeatIndex == i);
                var property = GetType().GetProperty($"Player{i + 1}");
                property?.SetValue(this, (player is null)?null : 
                    new PlayerViewModel(player)
                    {
                        IsCurrentPlayer = player.SeatIndex == currentPlayerIndex,
                        IsDealer = player.SeatIndex == dealerIndex
                    });
            }
        }
        public void UpsertPlayer(PlayerInfo p, string currentMove)
        {
        }
        public void RemovePlayer(Guid playerId)
        {

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
            BordViewModel = new BordViewModel();
        }
        private decimal pot;
    }
}
