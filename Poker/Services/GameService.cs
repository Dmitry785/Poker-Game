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
    public partial class GameService
    {
        public ILogger Logger = new NullLogger();
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
            ApplyConfig(config);
            cardDeck = new CardDeck();
            players = new PlayerListManager(maxPlayers);
        }
        private void ApplyConfig(GameConfig config)
        {
            startMoney = config.StartMoney;
            minBet = config.MinBet;
            maxBet = config.MaxBet;
            smallBling = config.SmallBling;
            bigBlind = config.BigBlind;
            maxPlayers = config.MaxPlayers;
        }
        public string ClientName
        {
            get => clientName;
            set
            {
                if (State is ConnectionState.Connected ||
                State is ConnectionState.Hosting)
                {
                    players.ChangedPlayerName(clientName, value);
                    _signalBus.Publish(new PlayerListChanged(players.Players));
                }
                clientName = value;
                //fix если я хост, то сообщить всем имя измен
            }
        }
        public string RoomName
        {
            get => roomName;
            set
            {
                roomName = value;
                _signalBus.Publish(new RoomNameChanged());
                if (State is ConnectionState.Hosting)
                {
                    //fix если я хост, то сообщить всем об изменении названия комнаты
                }
            }
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
                case ConnectionState.Connected:
                    return await HandleLocalCommand_Connected(command);
                case ConnectionState.Connecting:
                    return await HandleLocalCommand_Connecting(command);
                case ConnectionState.Hosting:
                    return await HandleLocalCommand_Hosting(command);
            }
            return false;
        }
        private async Task<bool> Connect(IPEndPoint hostEP)
        {
            try
            {
                connectionTcs = new TaskCompletionSource<bool>();
                State = ConnectionState.Connecting;
                hostEndPoint = hostEP;
                bool sendRes = await _connection.Send(hostEP, new ClientConnectData(ClientName));
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
        private PlayerListManager players;
        private decimal currentMaxBet = 0;
        private decimal lastRaiseStep = 0;
        private decimal startMoney;
        private decimal minBet;
        private decimal maxBet;
        private decimal smallBling;
        private decimal bigBlind;
        private int maxPlayers;
        private string clientName = "Player";
        private string roomName = $"Player's room";
        private ConnectionState state = ConnectionState.NotConnected;
        private readonly TcpConnection _connection;
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
            State = ConnectionState.Hosting;
            players.AddPlayer(new PlayerInfo(ClientName, startMoney, 0));
            gameStage = GameStage.None;
            cardDeck.ResetDeck();
            cardDeck.Shuffle();
            communityCards.AddCard(new PokerCard(PokerCardNumber.Ace, PokerCardSuit.Diamonds));
            while (communityCards.CanAddCard())
                communityCards.AddCard(cardDeck.GetCard());
            OnPlayerListChanged();
            OnRoundStageChanged();
        }
        private void OnDisconnected()
        {
            State = ConnectionState.NotConnected;
            gameStage = GameStage.None;
            hostEndPoint = null;
            players.Reset();
            communityCards.Reset();
            OnPlayerListChanged();
            OnRoundStageChanged();
        }
        private void ApplyGameState(GameState s)
        {
            communityCards.Cards = s.communityCards;
            currentPlayerIndex = s.currentPlayerIndex;
            RoomName = s.roomName;
            dealerIndex = s.dealerIndex;
            smallBling = s.smallBlind;
            bigBlind = s.bigBlind;
            minBet = s.minBet;
            pot = s.pot;
            players.Players = s.players;
            MessageBox.Show(string.Join(", ", players.Players.Select(x =>new { x.PlayerId, x.Name })));
            MessageBox.Show(s.playerId.ToString());
            players.CorrelateById(s.playerId);
            gameStage = s.stage;
            OnPlayerListChanged();
            OnRoundStageChanged();
        }
        private async Task HandleClientConnecting(IPEndPoint endPoint, ClientConnectData data)
        {
            if (State is not ConnectionState.Hosting)
                return;
            var playerInfo = new ConnectedPlayerInfo(data.name, startMoney, endPoint);
            Guid? playerId = players.AddPlayer(playerInfo);
            if (playerId is null)
            {
                await _connection.Send(endPoint, new ConnectionDeclined("Игроков слишком много, попробуйте позднее"));
                return;
            }
            OnPlayerListChanged();
            await _connection.Send(endPoint, new GameState(roomName, dealerIndex,
                smallBling, bigBlind, minBet, pot, communityCards.Cards,
                gameStage, currentPlayerIndex, players.Players.ToList(), (Guid)playerId));
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
                    _ = _connection.Send(hostEndPoint!, new ClientDisconnectData(c.reason));
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
                    _ = _connection.Send(hostEndPoint!, new ClientDisconnectData(c.reason));
                    OnDisconnected();
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
                    ApplyGameState(c);
                    break;
            }
        }
        private async Task OnMessageReceived_Connecting(IPEndPoint endPoint, DataTransferBase message)
        {
            if (endPoint.ToString() != hostEndPoint.ToString())
                return;
            switch (message)
            {
                case GameState c:
                    connectionTcs?.TrySetResult(true);
                    ApplyGameState(c);
                    break;
                case ConnectionDeclined c:
                    connectionTcs?.TrySetResult(false);
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
                    await HandleClientConnecting(endPoint, c);
                    break;
                case ClientDisconnectData c:
                    break;
            }
        }
        private void OnRoundStageChanged()
        {
            _signalBus.Publish(new RoundStageChanged(gameStage, communityCards.Cards, 0, 0));
        }
        private void OnPlayerListChanged()
        {
            _signalBus.Publish(new PlayerListChanged(players.Players));
        }
    }
    #endregion

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
}
