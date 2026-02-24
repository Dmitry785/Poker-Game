using Poker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker.ViewModels
{
    public class GameInfoViewModel : BaseViewModel
    {
        public string PlayerName
        {
            get => playerName;
            set
            {
                playerName = value;
                _game.ClientName = playerName;
                OnPropertyChanged();
            }
        }
        private GameService _game;
        public GameInfoViewModel(GameService game, SignalBus sb)
        {
            _game = game;
            playerName = _game.ClientName;
        }
        private string playerName;
    }
}
