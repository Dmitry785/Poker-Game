using Poker.Models;
using Poker.Services;
using Poker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public HandViewModel HandViewModel { get; set; }
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
        public void UpdatePlayers(List<PlayerInfo> players)
        {
            for (int i=0;i<6; i++)
            {
                PlayerInfo? player = players.FirstOrDefault(x => x.SeatIndex == i);
                SetPlayer(i, (player is null) ? null:new PlayerViewModel(player)
                {
                    IsDealer = player.SeatIndex == dealerIndex,
                    IsCurrentPlayer = player.SeatIndex == currentPlayerIndex
                });
            }
        }
        public void SetHand(List<PokerCard> hand)
        {
            HandViewModel.UpdateCards(hand);
        }
        public void SetDealerIndex(int newDealerIndex)
        {
            dealerIndex = newDealerIndex;
            var currentPlayer = _playerFields[currentPlayerIndex];
            for (int i = 0; i < 6; i++)
            {
                var player = _playerFields[i];
                if (player is null)
                    continue;
                player.IsDealer = i == dealerIndex;
                SetPlayer(i, player);
            }
        }
        public void SetCurrentPlayerIndex(int newCurrentPlayerIndex)
        {
            currentPlayerIndex = newCurrentPlayerIndex;
            var currentPlayer = _playerFields[currentPlayerIndex];
            for (int i = 0; i < 6; i++)
            {
                var player = _playerFields[i];
                if (player is null)
                    continue;
                player.IsCurrentPlayer = i == currentPlayerIndex;
                SetPlayer(i, player);
            }
        }
        private void SetPlayer(int seatIndex, PlayerViewModel? player)
        {
            switch (seatIndex)
            {
                case 0:
                    if (Player1 is null)
                        Player1 = player;
                    else if (player is null)
                        Player1 = null;
                    else
                        Player1.Update(player);
                    break;
                case 1:
                    if (Player2 is null)
                        Player2 = player;
                    else if (player is null)
                        Player2 = null;
                    else
                        Player2.Update(player);
                    break;
                case 2:
                    if (Player3 is null)
                        Player3 = player;
                    else if (player is null)
                        Player3 = null;
                    else
                        Player3.Update(player);
                    break;
                case 3:
                    if (Player4 is null)
                        Player4 = player;
                    else if (player is null)
                        Player4 = null;
                    else
                        Player4.Update(player);
                    break;
                case 4:
                    if (Player5 is null)
                        Player5 = player;
                    else if (player is null)
                        Player5 = null;
                    else
                        Player5.Update(player);
                    break;
                case 5:
                    if (Player6 is null)
                        Player6 = player;
                    else if (player is null)
                        Player6 = null;
                    else
                        Player6.Update(player);
                    break;
            }
        }
        /*public void UpsertPlayer(PlayerInfo p, int dealerIndex, int currentPlayerIndex)
        {
            SetPlayer(p.SeatIndex, new PlayerViewModel(p)
            {
                IsDealer = p.SeatIndex == dealerIndex,
                IsCurrentPlayer = p.SeatIndex == currentPlayerIndex
            });
        }
        public void RemovePlayer(Guid playerId)
        {
            var player = _playerFields.FirstOrDefault(x => x?.PlayerInfo.PlayerId == playerId);
            if (player is null)
                return;
            SetPlayer(player.PlayerInfo.SeatIndex, null);
        }*/
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
            HandViewModel = new HandViewModel();
        }
        private decimal pot;
        private int dealerIndex;
        private int currentPlayerIndex;
    }
}
