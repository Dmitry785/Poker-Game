using Poker.Connection;
using Poker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Poker.Services
{
    public partial class GameService
    {
        public ConnectionState State
        {
            get => state;
            private set
            {
                state = value;
                _signalBus.Publish(new StateChangedMessage());
            }
        }
        public IPAddress CurrentIP => _connection.CurrentIP;
        public GameService(SignalBus signalBus)
        {
            _connection = new TcpConnection();
            _connection.MessageReceived += OnMessageReceived;
            _signalBus = signalBus;
            cardDeck = new CardDeck();
        }

        private async void OnMessageReceived(IPEndPoint endPoint, DataTransferBase message)
        {
            switch (state)
            {
                case ConnectionState.NotConnected:
                    await OnMessageReceived_NotConnected(endPoint, message);
                    break;
                case ConnectionState.Connecting:
                    await OnMessageReceived_Connecting(endPoint, message);
                    break;
                case ConnectionState.Connected:
                    await OnMessageReceived_Connected(endPoint, message);
                    break;
                case ConnectionState.Hosting:
                    await OnMessageReceived_Hosting(endPoint, message);
                    break;
            }
        }
        public async Task<bool> HandleLocalCommand(GameCommand command)
        {
            switch (state)
            {
                case ConnectionState.NotConnected:
                    return await HandleLocalCommand_NotConnected(command);
                case ConnectionState.Connecting:
                    return await HandleLocalCommand_Connected(command);
                case ConnectionState.Connected:
                    return await HandleLocalCommand_Connecting(command);
                case ConnectionState.Hosting:
                    return await HandleLocalCommand_Hosting(command);
            }
            return false;
        }
        private async Task<bool> Connect(IPEndPoint hostIP)
        {
            try
            {
                connectionTcs = new TaskCompletionSource<bool>();
                State = ConnectionState.Connecting;
                hostEndPoint = hostIP;
                await _connection.Send(hostIP, new ClientMove(ClientMoveType.Connect));
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

                var completedTask = await Task.WhenAny(
                     connectionTcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    return false;
                }

                return await connectionTcs.Task;
            }
            catch
            {
                return false;
            }
            finally
            {
                connectionTcs = null;
            }
        }
        //отправить приглашение
        private CardDeck cardDeck;

        private ConnectionState state = ConnectionState.NotConnected;
        private readonly TcpConnection _connection;
        //для хоста
        private List<ConnectedPlayerInfo>? connectedPlayers;
        //для клиента
        private IPEndPoint? hostEndPoint;
        private readonly SignalBus _signalBus;
        private TaskCompletionSource<bool>? connectionTcs;
    }































    public partial class GameService
    {
        private async Task<bool> HandleLocalCommand_NotConnected(GameCommand command)
        {
            switch (command)
            {
                case ConnectCommand c:
                    if(await Connect(c.hostEndPoint))
                    {
                        State = ConnectionState.Connected;
                    }
                    else
                        State = ConnectionState.NotConnected;
                    break;
            }
            return false;
        }
        private async Task<bool> HandleLocalCommand_Connected(GameCommand command)
        {
            switch (command)
            {
                case BetCommand c:
                    break;
                case CallCommand c:
                    break;
                case FoldCommand c:
                    break;
                case CheckCommand c:
                    break;
                case RaiseCommand c:
                    break;
                case ConnectCommand c:
                    State = ConnectionState.Connecting;
                    await _connection.Send(c.hostEndPoint, new ClientMove(ClientMoveType.Connect));
                    break;
                case DisconnectCommand c:
                    State = ConnectionState.NotConnected;
                    _ = _connection.Send(hostEndPoint!, new ClientMove(ClientMoveType.Disconnect));
                    break;
            }
            return false;
        }
        private async Task<bool> HandleLocalCommand_Connecting(GameCommand command)
        {
            switch (command)
            {
                case BetCommand c:
                    break;
                case CallCommand c:
                    break;
                case FoldCommand c:
                    break;
                case CheckCommand c:
                    break;
                case RaiseCommand c:
                    break;
                case ConnectCommand c:
                    State = ConnectionState.Connecting;
                    await _connection.Send(c.hostEndPoint, new ClientMove(ClientMoveType.Connect));
                    break;
                case DisconnectCommand c:
                    State = ConnectionState.NotConnected;
                    _ = _connection.Send(hostEndPoint!, new ClientMove(ClientMoveType.Disconnect));
                    break;
            }
            return false;
        }
        private async Task<bool> HandleLocalCommand_Hosting(GameCommand command)
        {
            switch (command)
            {
                case BetCommand c:
                    break;
                case CallCommand c:
                    break;
                case FoldCommand c:
                    break;
                case CheckCommand c:
                    break;
                case RaiseCommand c:
                    break;
                case ConnectCommand c:
                    State = ConnectionState.Connecting;
                    await _connection.Send(c.hostEndPoint, new ClientMove(ClientMoveType.Connect));
                    break;
                case DisconnectCommand c:
                    State = ConnectionState.NotConnected;
                    _ = _connection.Send(hostEndPoint!, new ClientMove(ClientMoveType.Disconnect));
                    break;
            }
            return false;
        }
        private async Task OnMessageReceived_NotConnected(IPEndPoint endPoint, DataTransferBase message)
        {

        }
        private async Task OnMessageReceived_Connected(IPEndPoint endPoint, DataTransferBase message)
        {

        }
        private async Task OnMessageReceived_Connecting(IPEndPoint endPoint, DataTransferBase message)
        {
            switch (message)
            {
                case GameState c:
                    if (c.connectAccepted)
                        connectionTcs?.TrySetResult(true);
                    else
                        connectionTcs?.TrySetResult(false);
                    break;
            }
        }
        private async Task OnMessageReceived_Hosting(IPEndPoint endPoint, DataTransferBase message)
        {

        }
    }
}
