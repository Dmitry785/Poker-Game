using Poker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Collections.Concurrent;

namespace Poker.Services
{
    public static class CardsCacheManager
    {
        private static Dictionary<string, CardDeckCacheManager> _decks;
        static CardsCacheManager()
        {
            _decks = new Dictionary<string, CardDeckCacheManager>
                {
                    { "default", new DefaulCardDeckCache() }
                };
        }
        public static CardDeckCacheManager GetDeck(string deckname="default")
        {
            if (!_decks.TryGetValue(deckname, out var deck))
            {
                return _decks.First().Value;
            }
            return deck;
        }
    }
    public abstract class CardDeckCacheManager
    {
        public abstract BitmapSource GetCard(PokerCardNumber number, PokerCardSuit suit);
    }
    public class DefaulCardDeckCache : CardDeckCacheManager
    {
        private BitmapImage fullimage;
        private int cardWidth;
        private int cardHeight;
        private double gapX;
        private double gapY;
        private ConcurrentDictionary<(PokerCardNumber, PokerCardSuit), BitmapSource> _cards=new();
        public DefaulCardDeckCache()
        {
            cardWidth = 120;
            cardHeight = 170;
            gapX = 9.45;
            gapY = 11.5;
            fullimage = new BitmapImage(new Uri($"pack://application:,,,/data/decks/default/cards_sheet.png"));
        }
        private int GetColumn(PokerCardNumber n)
        {
            return (int)n;
        }
        private int GetRow(PokerCardSuit s)
        {
            //s0d2c3h1
            //d0c1h2s3
            int row = (int)s;
            switch (row)
            {
                case 0:
                    return 3;
                case 1:
                    return 2;
                case 2:
                    return 0;
                case 3:
                    return 1;
            }
            return 0;
        }
        public override BitmapSource GetCard(PokerCardNumber number, PokerCardSuit suit)
        {
            if(!_cards.TryGetValue((number, suit), out var img))
            {
                int row = GetRow(suit);
                int col = GetColumn(number);
                int x = (int)(col * cardWidth + gapX*col);
                int y = (int)(GetRow(suit) * cardHeight + gapY*row);
                img = new CroppedBitmap(fullimage, new Int32Rect(x, y, cardWidth, cardHeight));
                img.Freeze();
                _cards.TryAdd((number, suit), img);
            }
            return img;
        }
    }
}
