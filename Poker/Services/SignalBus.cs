using Poker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker.Services
{
    public class SignalBus
    {
        private readonly Dictionary<Type, List<Action<object>>> _subscribers = new();
        public void Subscribe<T>(Action<T> handler) where T : BaseMessage
        {
            if (!_subscribers.TryGetValue(typeof(T), out var handlers))
            {
                handlers = new List<Action<object>>();
                _subscribers[typeof(T)] = handlers;
            }
            handlers.Add(x => handler((T)x));
        }
        public void Publish<T>(T signal) where T : BaseMessage
        {
            if (_subscribers.TryGetValue(typeof(T), out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler(signal);
                }
            }
        }
    }
    #region SignalBusMessages
    public abstract class BaseMessage;
    public class PlayerListChanged : BaseMessage
    {
        public List<PlayerInfo> PlayerList;
        public PlayerListChanged(List<PlayerInfo> gameData)
        {
            PlayerList = gameData;
        }
    }
    public class PlayerStateChanged : BaseMessage
    {
        public int SeatIndex;
        public PlayerStatus Status;
        public decimal CurrentBet;
        public decimal Balance;
        public string Move;
        public PlayerStateChanged(int seatIndex, PlayerStatus status, decimal currentBet, decimal balance, string move)
        {
            SeatIndex = seatIndex;
            Status = status;
            CurrentBet = currentBet;
            Balance = balance;
            Move = move;
        }
    }
    public class RoundStageChanged : BaseMessage
    {
        public GameStage Stage;
        public CommunityCards Cards;
        public decimal Pot;
        public int DealerIndex;
        public RoundStageChanged(GameStage stage, CommunityCards cards, decimal pot, int dealerIndex)
        {
            Stage = stage;
            Cards = cards;
            Pot = pot;
            DealerIndex = dealerIndex;
        }
    }
    public class CardsReceived : BaseMessage
    {
        public HandCards Cards;
        public CardsReceived(HandCards cards)
        {
            Cards = cards;
        }
    }
    public class GameResultsOccurred : BaseMessage
    {
        public Dictionary<int, HandCards> Cards;
        public List<int> Winners;
        public decimal WinAmount;
        public GameResultsOccurred(Dictionary<int, HandCards> cards, List<int> winners, decimal winAmount)
        {
            Cards = cards;
            Winners = winners;
            WinAmount = winAmount;
        }
    }
    public class StateChangedMessage : BaseMessage{ }
    #endregion
}
