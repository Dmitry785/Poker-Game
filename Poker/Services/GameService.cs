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
using System.Windows;
using System.Xml.Linq;

namespace Poker.Services
{
    public class GameConfig
    {
        public decimal StartMoney;
        public decimal MinBet;
        public decimal MaxBet;
        public decimal SmallBling;
        public decimal BigBlind;
        public int MaxPlayers;
        public GameConfig(decimal startMoney, decimal minBet, decimal maxBet, decimal smallBling, decimal bigBlind, int maxPlayers)
        {
            StartMoney = startMoney;
            MinBet = minBet;
            MaxBet = maxBet;
            SmallBling = smallBling;
            BigBlind = bigBlind;
            MaxPlayers = maxPlayers;
        }
    }
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
        public IPEndPoint CurrentEndPoint => _connection.CurrentIP;
        public GameService(SignalBus signalBus, GameConfig config, TcpConnection connection)
        {
            _connection = connection;
            _connection.MessageReceived += OnMessageReceived;
            _signalBus = signalBus;
            _config = config;
            cardDeck = new CardDeck();
        }
        public string ClientName
        {
            get => clientName;
            set
            {
                clientName = value;
                if (State is ConnectionState.Connected ||
                State is ConnectionState.Hosting)
                {
                    players.First(x => x.SeatIndex == 0).Name = clientName;
                    _signalBus.Publish(new PlayerListChanged(players));
                }
                //fix если я хост, то сообщить всем имя измен
            }
        }
        private async void OnMessageReceived(IPEndPoint endPoint, DataTransferBase message)
        {
            MessageBox.Show($"message: {message.ToString()}", "Wow",
                MessageBoxButton.OK, MessageBoxImage.Hand);
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
                bool sendRes = await _connection.Send(hostIP, new ClientConnectData(ClientName));
                if (!sendRes)
                {
                    return false;
                }
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
        private CardDeck cardDeck = new CardDeck();
        private CommunityCards communityCards = new CommunityCards();
        private decimal pot = 0;
        private int currentPlayerIndex = 0;
        private int dealerIndex = 0;
        private GameStage gameStage = GameStage.None;
        private List<PlayerInfo> players = new List<PlayerInfo>();
        private decimal currentMaxBet = 0;
        private decimal lastRaiseStep;

        private string clientName = "Player";
        private ConnectionState state = ConnectionState.NotConnected;
        private GameConfig _config;
        private readonly TcpConnection _connection;
        //для хоста
        private List<ConnectedPlayerInfo>? connectedPlayers;
        //для клиента
        private IPEndPoint? hostEndPoint;
        private readonly SignalBus _signalBus;
        private TaskCompletionSource<bool>? connectionTcs;
    }
    public enum GameStage
    {
        None,
        PreFlop,
        Flop,
        Turn,
        River,
        Showdown
    }






























    public partial class GameService
    {
        private void OnStartHosting()
        {
            connectedPlayers = new List<ConnectedPlayerInfo>();
            players.Add(new PlayerInfo(ClientName, _config.StartMoney, 0));
            gameStage = GameStage.None;
            _signalBus.Publish(new PlayerListChanged(players));
            _signalBus.Publish(new RoundStageChanged(gameStage, communityCards, 0, 0));
        }
        private void OnDisconnected()
        {
            gameStage = GameStage.None;
            hostEndPoint = null;
            players.Clear();
            _signalBus.Publish(new PlayerListChanged(players));
            _signalBus.Publish(new RoundStageChanged(gameStage, communityCards, 0, 0));
        }
        private void OnConnected(GameState gameState)
        {
            MessageBox.Show($"OnConnected: подключились к {hostEndPoint}");
        }
        #region HandleLocalCommand
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
                case StartHostCommand c:
                    State = ConnectionState.Hosting;
                    OnStartHosting();
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
                case DisconnectCommand c:
                    State = ConnectionState.NotConnected;
                    _ = _connection.Send(hostEndPoint!, new ClientDisconnectData(""));
                    OnDisconnected();
                    break;
            }
            return false;
        }
        private async Task<bool> HandleLocalCommand_Connecting(GameCommand command)
        {
            switch (command)
            {
                case DisconnectCommand c:
                    State = ConnectionState.NotConnected;
                    _ = _connection.Send(hostEndPoint!, new ClientDisconnectData(""));
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
                case DisconnectCommand c:
                    State = ConnectionState.NotConnected;
                    //отослать всем клиентам об завершении (disconnect all)
                    OnDisconnected();
                    break;
            }
            return false;
        }
        #endregion
        #region OnMessageReceivedCommand
        private async Task OnMessageReceived_NotConnected(IPEndPoint endPoint, DataTransferBase message)
        {
            //может прийти пришлашение
        }
        private async Task OnMessageReceived_Connected(IPEndPoint endPoint, DataTransferBase message)
        {
            if (endPoint != hostEndPoint)
                return;
            switch (message)
            {
                case GameUpdated c:
                    break;
                case GameState c:
                    break;
            }
        }
        private async Task OnMessageReceived_Connecting(IPEndPoint endPoint, DataTransferBase message)
        {
            if (endPoint != hostEndPoint)
                return;
            switch (message)
            {
                case GameState c:
                    connectionTcs?.TrySetResult(true);
                    OnConnected(c);
                    break;
                case ConnectionDeclined c:
                    connectionTcs?.TrySetResult(false);
                    MessageBox.Show($"Не удалось подключиться: {c.reason}");
                    break;
            }
        }
        private async Task OnMessageReceived_Hosting(IPEndPoint endPoint, DataTransferBase message)
        {
            switch (message)
            {
                case ClientMove c:
                    break;
                case ClientConnectData c:
                    players.Add(new PlayerInfo(clientName, _config.StartMoney, 1));
                    _signalBus.Publish(new PlayerListChanged(players));
                    await _connection.Send(endPoint, new GameState("current room", 0, 0, 0, 0, 0, communityCards, gameStage, currentPlayerIndex, null, players));
                    MessageBox.Show($"{c.name} подключился");
                    break;
                case ClientDisconnectData c:
                    break;
            }
        }
    }
    #endregion
}
