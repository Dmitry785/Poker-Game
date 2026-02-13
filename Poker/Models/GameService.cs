using Poker.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Poker.Models
{
    public class GameService
    {
        private ConnectionState _state = ConnectionState.NotConnected;
        private TcpConnection _connection;
        private List<TcpClient>? _connectedClients;
        private TcpClient? _hostClient;

        public event Action? GameChanged;
        public GameService()
        {
            _connection = new TcpConnection();
            _connection.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(TcpClient client, DataTransferBase message)
        {

        }
        public void HandleLocalCommand(GameCommand command)
        {

        }
        //отправить приглашение
    }
}
