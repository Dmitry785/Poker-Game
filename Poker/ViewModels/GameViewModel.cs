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
        public ICommand SomeCommand { get; }
        public GameViewModel(GameService game, SignalBus sb)
        {
            _game = game;
            SomeCommand = new Command(() =>
            {
                MessageBox.Show("Hello");
            });
        }
        private void HandleGameChanged()
        {

        }
    }
}
