using Poker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Poker.ViewModels
{
    public class PlayerViewModel : BaseViewModel
    {
        public PlayerInfo PlayerInfo;
        private string currentMove = string.Empty;
        public string Name => PlayerInfo.Name;
        public decimal Money => PlayerInfo.Money;
        public int TimeLeft { get; set; }
        public int MaxTime { get; set; }
        public void Update(PlayerViewModel player)
        {
            PlayerInfo = player.PlayerInfo;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(Money));
            CurrentMove = PlayerInfo.CurrentMove;
            IsDealer = player.IsDealer;
            OnPropertyChanged(nameof(IsDealer));
            IsCurrentPlayer = player.IsCurrentPlayer;
            OnPropertyChanged(nameof(IsCurrentPlayer));
            OnPropertyChanged(nameof(CurrentMoveVisibility));
            OnPropertyChanged(nameof(Status));
        }
        public string CurrentMove
        {
            get => currentMove;
            set
            {
                currentMove = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentMoveVisibility));
            }
        }
        public Visibility CurrentMoveVisibility => (currentMove == string.Empty) ? 
            Visibility.Collapsed : Visibility.Visible;
        public bool IsCurrentPlayer { get; set; } = false;
        public bool IsDealer { get; set; } = false;
        public PlayerStatus Status { get; private set; }
        public PlayerViewModel(PlayerInfo playerInfo)
        {
            PlayerInfo = playerInfo;
            Status = playerInfo.Status;
        }
    }
}
