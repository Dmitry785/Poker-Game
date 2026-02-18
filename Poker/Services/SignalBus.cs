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
    public class GameUpdatedMessage
    {
        public string GameData;
        public GameUpdatedMessage(string gameData)
        {
            GameData = gameData;
        }
    }
    public class StateChangedMessage : BaseMessage{ }
    #endregion
}
