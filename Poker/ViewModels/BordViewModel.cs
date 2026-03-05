using Poker.Models;
using Poker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Poker.ViewModels
{
    public class BordViewModel : BaseViewModel
    {
        public BordViewModel()
        {

        }
        public void ClearCards()
        {
            Card1 = null;
            Card2 = null;
            Card3 = null;
            Card4 = null;
            Card5 = null;
        }
        public void UpdateCards(List<PokerCard> cards)
        {
            ClearCards();
            if (cards.Count > 0)
                Card1 = new CardViewModel(cards[0]);
            if (cards.Count > 1)
                Card2 = new CardViewModel(cards[1]);
            if (cards.Count > 2)
                Card3 = new CardViewModel(cards[2]);
            if (cards.Count > 3)
                Card4 = new CardViewModel(cards[3]);
            if (cards.Count > 4)
                Card5 = new CardViewModel(cards[4]);
        }
        public CardViewModel? Card1
        {
            get => card1;
            set
            {
                card1 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Card1Visibility));
            }
        }
        public CardViewModel? Card2
        {
            get => card2;
            set
            {
                card2 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Card2Visibility));
            }
        }
        public CardViewModel? Card3
        {
            get => card3;
            set
            {
                card3 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Card3Visibility));
            }
        }
        public CardViewModel? Card4
        {
            get => card4;
            set
            {
                card4 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Card4Visibility));
            }
        }
        public CardViewModel? Card5
        {
            get => card5;
            set
            {
                card5 = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Card5Visibility));
            }
        }
        public Visibility Card1Visibility => (Card1 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Card2Visibility => (Card2 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Card3Visibility => (Card3 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Card4Visibility => (Card4 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Card5Visibility => (Card5 is null) ? Visibility.Hidden : Visibility.Visible;
        private CardViewModel? card1;
        private CardViewModel? card2;
        private CardViewModel? card3;
        private CardViewModel? card4;
        private CardViewModel? card5;
    }
}
