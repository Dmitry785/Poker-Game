using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Poker.Services
{
    public interface ILogger
    {
        void Message(string message);
        void Error(string message, string error);
    }
    public interface IHubbedLogger : ILogger
    {
        event Action<string>? MessageReceived;
        event Action<string, string>? ErrorReceived;
    }
    public class MessageBoxLogger : ILogger
    {
        public void Error(string message, string error)
        {
            Task.Run(() => MessageBox.Show(message));
        }

        public void Message(string message)
        {
            Task.Run(() =>MessageBox.Show(message));
        }
    }
    public class HubbedLogger : IHubbedLogger
    {
        public event Action<string>? MessageReceived;
        public event Action<string, string>? ErrorReceived;

        public void Error(string message, string error)
        {
            ErrorReceived?.Invoke(message, error);
        }

        public void Message(string message)
        {
            MessageReceived?.Invoke(message);
        }
    }
    public class ListBoxLoggerHub : ListBoxLogger
    {
        private Dictionary<IHubbedLogger, string> _loggerPrefixes;
        public ListBoxLoggerHub(ICollection<string> list, Dictionary<IHubbedLogger, string> loggers)
            :base(list)
        {
            _loggerPrefixes = loggers;
            foreach(var hubbed in _loggerPrefixes)
            {
                hubbed.Key.MessageReceived += (mes) =>
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        collection.Add($"{hubbed.Value} >> {mes};");
                    });
                };
                hubbed.Key.ErrorReceived += (mes, err) =>
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        collection.Add($"{hubbed.Value} !! {mes}; ERR: {err}");
                    });
                };
            }
        }
    }
    public class ListBoxLogger : ILogger
    {
        protected ICollection<string> collection;
        public ListBoxLogger(ICollection<string> list)
        {
            collection = list;
        }
        public void Error(string message, string error)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                collection.Add($"{message} ERR: {error}");
            });
        }

        public void Message(string message)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                collection.Add(message);
            });
        }
    }
    public class NullLogger : ILogger
    {
        public void Error(string message, string error)
        {
        }

        public void Message(string message)
        {
        }
    }
}
