using Poker.Models;
using Poker.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Poker.ViewModels
{
    public class CardViewModel : BaseViewModel
    {
        public string CardNumber { get; set; }
        public string CardSuit { get; set; }
        public SolidColorBrush CardColor { get; set; }
        private BitmapSource cardImage;
        public BitmapSource CardImage
        {
            get => cardImage;
            set
            {
                cardImage = value;
                OnPropertyChanged();
            }
        }
        public CardViewModel(PokerCard card)
        {
            CardImage = CardsCacheManager.GetDeck().GetCard(card.CardNumber, card.CardSuit);
            
            //optimize it
            CardNumber = card.CardNumber.ToString();
            CardSuit = card.CardSuit.ToString();
            CardColor = new SolidColorBrush(Colors.Red);

        }
    }
}
