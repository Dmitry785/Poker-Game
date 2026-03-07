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
        public int CurrentPlayerIndex => currentPlayerIndex;
        public GameStage CurrentGameStage => gameStage;
        public decimal CurrentMaxBet => maxBet;
        public List<PokerCard> CommunityCards => communityCards.Cards;
        public int DealerIndex => dealerIndex;
        public decimal Pot => pot;
        public decimal MinBetRaise
        {
            get
            {
                if (currentMaxBet == 0)
                    return bigBlind;
                return currentMaxBet + lastRaiseStep;
            }
        }
        public List<PlayerInfo> Players=> players.Players;
        public PlayerInfo? LocalPlayer => players.Players.Find(x => x.SeatIndex == 0);
        public object CurrentAddress => _connection.CurrentAddress;
        public GameService(SignalBus signalBus, GameConfig config, ConnectionManager connection)
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
            //minBet = config.MinBet;
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
                    OnPlayerListChanged();
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
        private decimal pot;
        private int currentPlayerIndex;
        private int dealerIndex;
        private GameStage gameStage = GameStage.None;
        private PlayerListManager players;
        private decimal currentMaxBet;
        private decimal lastRaiseStep;
        private decimal startMoney;
        private decimal maxBet;
        private decimal smallBling;
        private decimal bigBlind;
        private int maxPlayers;
        private string clientName = "Player";
        private string roomName = $"Player's room";
        private ConnectionState state = ConnectionState.NotConnected;
        private bool isGameHasStartedOnce = false;

        private readonly ConnectionManager _connection;
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
            dealerIndex = 0;
            currentPlayerIndex = 0;
            isGameHasStartedOnce = false;
            pot = 0;
            gameStage = GameStage.None;
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
        private void ApplyGameStateUpdated(GameStateUpdated s)
        {
            communityCards.Cards = s.communityCards;
            dealerIndex = s.dealerIndex;
            currentPlayerIndex = s.currentPlayerIndex;
            pot = s.pot;
            gameStage = s.stage;
            OnPlayerListChanged();
            OnRoundStageChanged();
        }
        private void ApplyGameStateAll(GameStateAll s)
        {
            communityCards.Cards = s.communityCards;
            RoomName = s.roomName;
            dealerIndex = s.dealerIndex;
            currentPlayerIndex = s.currentPlayerIndex;
            smallBling = s.smallBlind;
            bigBlind = s.bigBlind;
            //добавить maxPlayers (не критично, тк пока макс = 6)
            pot = s.pot;
            int seatsCorrelationDifference = s.players.Find(x => x.PlayerId == s.playerId)!.SeatIndex;
            CorrelatePlayerIndex(ref dealerIndex, seatsCorrelationDifference);
            CorrelatePlayerIndex(ref currentPlayerIndex, seatsCorrelationDifference);
            players.Players = s.players;
            players.CorrelateById(s.playerId);
            gameStage = s.stage;
            OnPlayerListChanged();
            OnRoundStageChanged();
        }
        private void CorrelatePlayerIndex(ref int playerIndex, int difference)
        {
            playerIndex = (maxPlayers - difference + playerIndex) % maxPlayers;
        }
        private async Task OnGameStarted()
        {
            foreach(PlayerInfo p in players.Players)
            {
                if (p.Money <= 0)
                    p.Status = PlayerStatus.Out;
                else
                    p.Status = PlayerStatus.Active;
            }
            if (players.Players.Count <= 1 || gameStage != GameStage.None || State is not ConnectionState.Hosting)
                return;
            pot = 0;
            if (!isGameHasStartedOnce)
            {
                SetRandomPlayerIndex(ref dealerIndex);
                isGameHasStartedOnce = true;
            }
            else
            {
                SetNextActivePlayerIndex(ref dealerIndex);
            }
            int smallBlindPlayerIndex = dealerIndex,
                bigBlindPlayerIndex = dealerIndex;
            SetNextActivePlayerIndex(ref smallBlindPlayerIndex, 1);
            SetNextActivePlayerIndex(ref bigBlindPlayerIndex, 2);
            CarryOutSmallAndBigBlinds(smallBlindPlayerIndex, bigBlindPlayerIndex);
            await DealCards();
            currentPlayerIndex = bigBlindPlayerIndex;
            SetNextActivePlayerIndex(ref currentPlayerIndex, 1);
            gameStage = GameStage.PreFlop;
            await SendToAll(new GameStateUpdated
                (dealerIndex, currentMaxBet, lastRaiseStep,
                pot,communityCards.Cards, gameStage, currentPlayerIndex));
            OnRoundStageChanged();
        }
        private async Task DealCards()
        {
            cardDeck.ResetDeck();
            cardDeck.Shuffle();

            for(int i = 0; i < players.Players.Count; i++)
            {
                PokerCard c1 = cardDeck.GetCard(),
                    c2 = cardDeck.GetCard();
                players.Players[i].SetHand(c1, c2);
                if (players.Players[i] is ConnectedPlayerInfo cplayer)
                {
                    await _connection.Send(cplayer.ClientEndPoint, new DealCardsData(new List<PokerCard>()
                    {
                        c1,c2
                    }));
                }
            }
        }
        private void CarryOutSmallAndBigBlinds(int bbIndex, int sbIndex)
        {
            var sbPlayer = players.Players.First(x => x.SeatIndex == sbIndex);
            var bbPlayer = players.Players.First(x => x.SeatIndex == bbIndex);

            decimal sbAmount = Math.Min(smallBling, sbPlayer.Money);
            sbPlayer.Money -= sbAmount;
            sbPlayer.CurrentBet = sbAmount;
            if (sbPlayer.Money == 0)
                sbPlayer.Status = PlayerStatus.AllIn;

            decimal bbAmount = Math.Min(bigBlind, bbPlayer.Money);
            sbPlayer.Money -= sbAmount;
            sbPlayer.CurrentBet = sbAmount;
            if (sbPlayer.Money == 0)
                sbPlayer.Status = PlayerStatus.AllIn;

            lastRaiseStep = bigBlind;
            currentMaxBet = bigBlind;
        }

        private void SetRandomPlayerIndex(ref int playerIndex)
        {
            var allPlayersSeats = players.Players.Select(x => x.SeatIndex).ToList();
            if (!allPlayersSeats.Any())
                throw new Exception();
            playerIndex = allPlayersSeats[Random.Shared.Next(allPlayersSeats.Count)];
        }
        private void SetNextActivePlayerIndex(ref int playerIndex, int step = 1)
        {
            var allPlayers = players.Players.
                OrderBy(x => x.SeatIndex).ToList();

            int playerIndexCopy = playerIndex;
            var currentIndex = allPlayers.FindIndex(x => x.SeatIndex == playerIndexCopy);
            if (currentIndex < 0)
                currentIndex = 0;


            int attempts = 0;
            while (attempts < allPlayers.Count)
            {
                currentIndex = (currentIndex + 1) % allPlayers.Count;
                var player = allPlayers[currentIndex];

                if (player.Status == PlayerStatus.Active)
                {
                    playerIndex = player.SeatIndex;
                    return;
                }
                attempts++;
            }
        }
        private void SetNext(ref int n, int max, int step = 1)
        {
            n = (n + step) % max;
        }
        private bool CanNextGameStage()
        {
            return false;
        }
        private void OnNextGameStage()
        {
            if (!CanNextGameStage())
                return;
            pot += players.Players.Sum(x => x.CurrentBet);
            currentMaxBet = 0;
            lastRaiseStep = bigBlind;
        }
        private async Task HandleClientConnecting(IPEndPoint endPoint, ClientConnectData data)
        {
            if (players.Players.Exists(x =>
            {
                if (x is ConnectedPlayerInfo p)
                    return p.ClientEndPoint.ToString() == endPoint.ToString();
                return false;
            }))
            {
                await _connection.Send(endPoint, new ConnectionDeclined(
                    "Вы уже в игре, попробуйте подключиться позднее"));
                return;
            }
            var playerInfo = new ConnectedPlayerInfo(data.name, startMoney, endPoint);
            Guid? playerId = players.AddPlayer(playerInfo);
            if (playerId is null)
            {
                await _connection.Send(endPoint, new ConnectionDeclined(
                    "Игроков слишком много, попробуйте позднее"));
                return;
            }
            playerInfo.PlayerId = (Guid)playerId;
            await _connection.Send(endPoint, new GameStateAll(roomName, dealerIndex,
                smallBling, bigBlind, currentMaxBet, lastRaiseStep, pot, communityCards.Cards,
                gameStage, currentPlayerIndex, players.Players, (Guid)playerId));
            await SendToAll(new ClientConnected(playerInfo));
            MessageBox.Show($"Connect {endPoint}");
            OnPlayerListChanged();
        }
        private async Task SendToAll(DataTransferBase message)
        {
            if (State is not ConnectionState.Hosting)
                return;
            foreach(PlayerInfo player in players.Players)
            {
                if (player is ConnectedPlayerInfo cp)
                    await _connection.Send(cp.ClientEndPoint, message);
            }
        }
        private void HandleCheck(int playerSeat)
        {
        }
        private void HandleCall(int playerSeat)
        {
            var player = players.Players.Find(x => x.SeatIndex == playerSeat);
            if (player is null)
                return;
            if (currentPlayerIndex != playerSeat)
                return;
            if (player.Money < currentMaxBet)
                return;
        }
        private void HandleBetRaise(int playerSeat, decimal bet)
        {
            var player = players.Players.Find(x => x.SeatIndex == playerSeat);
            if (player is null)
                return;
            if (currentPlayerIndex != playerSeat)
                return;

            if (bet < MinBetRaise)
            {
                return;
            }

            decimal newLastRaiseStep = bet - currentMaxBet;

            if (currentMaxBet == 0)
                lastRaiseStep = Math.Max(newLastRaiseStep, bigBlind);
            else
                lastRaiseStep = newLastRaiseStep;
            currentMaxBet = bet;
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
                case CallCommand c:
                    break;
                case FoldCommand c:
                    break;
                case CheckCommand c:
                    break;
                case BetRaiseCommand c:
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
                case StartGameCommand c:
                    await OnGameStarted();
                    break;
                case CallCommand c:
                    break;
                case FoldCommand c:
                    break;
                case CheckCommand c:
                    break;
                case BetRaiseCommand c:
                    HandleBetRaise(0, c.amount);
                    break;
                case DisconnectCommand c:
                    //отослать всем клиентам об завершении (disconnect all)
                    await SendToAll(new GameUpdated(LocalPlayer!.PlayerId, GameUpdatedType.Disconnected));
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
                case GameStateUpdated c:
                    ApplyGameStateUpdated(c);
                    break;
                    /*case GameState c:
                        ApplyGameState(c);
                        break;*/
            }
        }
        private async Task OnMessageReceived_Connecting(IPEndPoint endPoint, DataTransferBase message)
        {
            if (endPoint.ToString() != hostEndPoint!.ToString())
                return;
            switch (message)
            {
                case GameStateAll c:
                    connectionTcs?.TrySetResult(true);
                    ApplyGameStateAll(c);
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
            _signalBus.Publish(new RoundStageChanged());
        }
        private void OnPlayerListChanged()
        {
            _signalBus.Publish(new PlayerListChanged());
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
