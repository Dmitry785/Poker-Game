using Poker.Models;
using Poker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Poker.ViewModels
{
    public class HandViewModel : BaseViewModel
    {
        public HandViewModel()
        {
        }
        public void ClearCards()
        {
            Card1 = null;
            Card2 = null;
        }
        public void UpdateCards(List<PokerCard> cards)
        {
            ClearCards();
            for(int i=0;i<2 && i < cards.Count; i++)
            {
                var property = GetType().GetProperty($"Card{i + 1}");
                property?.SetValue(this, new CardViewModel(cards[i]));
            }
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
        public Visibility Card1Visibility => (Card1 is null) ? Visibility.Hidden : Visibility.Visible;
        public Visibility Card2Visibility => (Card2 is null) ? Visibility.Hidden : Visibility.Visible;
        private CardViewModel? card1;
        private CardViewModel? card2;
    }
}
