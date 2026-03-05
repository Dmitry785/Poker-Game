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
            get => _game.ClientName;
            set
            {
                _game.ClientName = value;
                OnPropertyChanged();
            }
        }
        private GameService _game;
        public GameInfoViewModel(GameService game, SignalBus sb)
        {
            _game = game;
        }
    }
}
