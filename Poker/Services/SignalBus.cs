using Poker.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poker.Connection;

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
    public abstract record BaseMessage;
    public record ChatMessageReceivedMessage(string message, string senderName) : BaseMessage;
    public record PlayersUpdatedMessage(List<PlayerInfo> players) : BaseMessage;
    public record PrivateCardDealtMessage(List<PokerCard> hand) : BaseMessage;
    public record TableStateChangedMessage(List<PokerCard> communityCards,
        GameStage stage, decimal pot, int dealerIndex) : BaseMessage;
    public record PlayerTurnMessage(int currentPlayerIndex, decimal currentMaxBet) : BaseMessage;
    public record GameResultsOccurredMessage(Dictionary<Guid, HandCards> cards, 
        List<WinnerInfo> winners) : BaseMessage;
    public record StateChangedMessage(Connection.ConnectionState state) : BaseMessage;
    #endregion
}
