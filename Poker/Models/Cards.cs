using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Poker.Models
{
    public class PokerCard
    {
        public PokerCardNumber CardNumber { get; set; }
        public PokerCardSuit CardSuit { get; set; }
        public PokerCard()
        { 
        }
        public PokerCard(PokerCardNumber number,  PokerCardSuit suit)
        {
            CardNumber = number;
            CardSuit = suit;
        }
    }
    public enum PokerCardNumber
    {
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }
    public enum PokerCardSuit
    {
        Sprades,
        Hearts,
        Diamonds,
        Clubs
    }
    public class CardDeck
    {
        private static readonly PokerCard[] _cards = 
        {
            new PokerCard(PokerCardNumber.Two, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Two, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Two, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Two, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Three, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Three, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Three, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Three, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Four, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Four, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Four, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Four, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Five, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Five, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Five, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Five, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Six, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Six, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Six, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Six, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Seven, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Seven, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Seven, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Seven, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Eight, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Eight, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Eight, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Eight, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Nine, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Nine, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Nine, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Nine, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Ten, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Ten, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Ten, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Ten, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Jack, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Jack, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Jack, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Jack, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Queen, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Queen, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Queen, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Queen, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.King, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.King, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.King, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.King, PokerCardSuit.Clubs),

            new PokerCard(PokerCardNumber.Ace, PokerCardSuit.Sprades),
            new PokerCard(PokerCardNumber.Ace, PokerCardSuit.Hearts),
            new PokerCard(PokerCardNumber.Ace, PokerCardSuit.Diamonds),
            new PokerCard(PokerCardNumber.Ace, PokerCardSuit.Clubs)
        };
        private List<PokerCard> cardDeck = new List<PokerCard>();
        public CardDeck()
        {
            
        }
        public int CardsInDeck()
        {
            return cardDeck.Count;
        }
        public void ResetDeck()
        {
            cardDeck = _cards.ToList();
        }
        public void Shuffle()
        {
            int n = cardDeck.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Shared.Next(n + 1);
                PokerCard v = cardDeck[k];
                cardDeck[k] = cardDeck[n];
                cardDeck[n] = v;
            }
        }
        public bool IsEmpty()
        {
            return cardDeck.Count == 0;
        }
        public PokerCard GetCard()
        {
            if (IsEmpty())
                throw new Exception();
            PokerCard card = cardDeck.Last();
            cardDeck.RemoveAt(cardDeck.Count - 1);
            return card;
        }
    }
    public class HandCards
    {
        public PokerCard Card1 { get; set; }
        public PokerCard Card2 { get; set; }
        public List<PokerCard> Cards => new List<PokerCard>() { Card1, Card2 };
        public HandCards(PokerCard card1, PokerCard card2)
        {
            Card1 = card1;
            Card2 = card2;
        }
    }
    public class CommunityCards
    {
        private List<PokerCard> cards = new();
        public List<PokerCard> Cards
        {
            get => cards.ToList();
            set
            {
                for(int i = 0; i < value.Count && CanAddCard(); i++)
                {
                    AddCard(value[i]);
                }
            }
        }
        public CommunityCards()
        {

        }
        public void Reset()
        {
            cards = new();
        }
        public bool CanAddCard()
        {
            return cards.Count < 5;
        }
        public void AddCard(PokerCard card)
        {
            if (!CanAddCard())
                throw new Exception();
            cards.Add(card);
        }
        
    }
    
}
