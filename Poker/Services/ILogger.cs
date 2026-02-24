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
    }
    public class MessageBoxLogger : ILogger
    {
        public void Message(string message)
        {
            Task.Run(() =>MessageBox.Show(message));
        }
    }
    public class ListBoxLogger : ILogger
    {
        private ICollection<string> collection;
        public ListBoxLogger(ICollection<string> list)
        {
            collection = list;
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
        public void Message(string message)
        {
        }
    }
}
