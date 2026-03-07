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
        private PlayerInfo info;
        private string currentMove = string.Empty;
        public string Name => info.Name;
        public decimal Money => info.Money;
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
            info = playerInfo;
            Status = playerInfo.Status;
        }
    }
}
