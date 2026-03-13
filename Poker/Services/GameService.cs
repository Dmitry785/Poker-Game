using Poker.Connection;
using Poker.Models;
using Poker.Views;
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
        public ILogger GameLogger = new NullLogger();
        public ConnectionState State => _connection.State;
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
        public PlayerInfo? LocalPlayer => players.Players.Find(x => x.PlayerId == SelfPlayerId);
        public object CurrentAddress => _connection.CurrentAddress;
        public GameService(SignalBus signalBus, GameConfig config, ConnectionManager connection)
        {
            _connection = connection;
            _connection.ClientConnecting += HandleClientConnecting;
            _connection.ClientNewMessage += HandleClientNewMessage;
            _connection.MessageFromHost += HandleMessageFromHost;
            _connection.StateChanged += OnStateChanged;
            _signalBus = signalBus;
            ApplyConfig(config);
            cardDeck = new CardDeck();
            players = new PlayerListManager(maxPlayers);
        }
        private async void HandleClientConnecting(ClientConnectData cd, IPEndPoint endPoint)
        {
            PlayerInfo connectingClient = new PlayerInfo(cd.name, startMoney);
            
            Guid? playerId = players.AddPlayer(connectingClient);
            if (playerId is null)
            {
                await _connection.SendRaw(endPoint, new ConnectionDeclined("Слишком много игроков"));
                return;
            }
            connectingClient.PlayerId = (Guid)playerId;
            await _connection.SendBroadcast(new ClientConnected(connectingClient));
            _connection.RegisterPlayer(connectingClient.PlayerId, endPoint);
            await _connection.Send(connectingClient.PlayerId, new GameStateAll(roomName, dealerIndex,
                smallBling, bigBlind, currentMaxBet, lastRaiseStep, pot, communityCards.Cards,
                gameStage, currentPlayerIndex, players.Players, connectingClient.PlayerId));
            OnPlayersUpdated();
        }
        private async void HandleClientNewMessage(Guid playerId, DataTransferBase message)
        {
            Logger.Message($"Сообщение от {playerId} {message}");
            PlayerInfo p = players.Players.First(x => x.PlayerId == playerId);
            switch (message)
            {
                case ClientMove c:
                    _timerCts?.Cancel();
                    break;
                case ClientDisconnectData c:
                    break;
                case ClientChatMessaged c:
                    await _connection.SendBroadcast(new GameChatMessaged(playerId, c.message));
                    break;
                case ClientReconnectData c://cs
                    break;
            }
        }
        private async void HandleMessageFromHost(DataTransferBase message)
        {
            Logger.Message($"Сообщение от хоста {message}");
            switch (message)
            {
                case DealCardsData c:
                    LocalPlayer!.SetHand(c.hand);
                    OnPrivateCardDealt();
                    break;
                case GameStateAll c:
                    ApplyGameStateAll(c);
                    break;
                case GameHostingClosed c:
                    if (State is ConnectionState.Hosting)
                        break;
                    await Disconnect();
                    break;
                case GameChatMessaged c:
                    OnGameChatMessaged(players.GetById(c.senderId)!.Name, c.message);
                    break;
                case GameUpdated c:
                    break;
                case ClientConnected c:
                    break;
                case GameStateUpdated c:
                    break;
                case ConnectionDeclined c:
                    Logger.Error("Не удалось подключиться к хосту", c.reason);
                    break;
            }
            /*
    public record InviteData(IPEndPoint hostEndPoint) : DataTransferBase;ифровать
    public record ConnectionDeclined(string reason) : DataTransferBase;*/
        }
        private void ApplyConfig(GameConfig config)
        {
            startMoney = config.StartMoney;
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
                if (LocalPlayer is not null)
                {
                    LocalPlayer.Name = value;
                    OnPlayersUpdated();
                }
                clientName = value;
            }
        }
        public string RoomName
        {
            get => roomName;
            set
            {
                roomName = value;
            }
        }
        public async Task HandleLocalCommand(GameCommand command)
        {
            Logger.Message($"Действие {command}");
            switch (command)
            {
                case DisconnectCommand c:
                    if (State is ConnectionState.NotConnected)
                        break;
                    await Disconnect();
                    break;
                case ConnectCommand c:
                    if (State is not ConnectionState.NotConnected)
                        break;
                    await Connect(c.hostEndPoint);
                    break;
                case StartHostCommand c:
                    if (State is not ConnectionState.NotConnected)
                        break;
                    StartHost();
                    break;
                default:
                    if (State is ConnectionState.NotConnected 
                        or ConnectionState.Connecting)
                        break;
                    switch (command)
                    {
                        case BetRaiseCommand c:
                            await _connection.SendToHost(new ClientMove(ClientMoveType.BetRaise, c.amount));
                            break;
                        case SendCommonMessage c:
                            await _connection.SendToHost(new ClientChatMessaged(c.message));
                            break;
                        case CallCommand c:
                            await _connection.SendToHost(new ClientMove(ClientMoveType.Call));
                            break;
                        case FoldCommand c:
                            await _connection.SendToHost(new ClientMove(ClientMoveType.Fold));
                            break;
                        case CheckCommand c:
                            await _connection.SendToHost(new ClientMove(ClientMoveType.Check));
                            break;
                        case StartGameCommand c:
                            await StartGame();
                            break;
                    }
                    break;
            }
        }
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
        private bool isGameHasStartedOnce = false;
        private CancellationTokenSource? _timerCts;
        private int secondsToMove = 15;
        private Guid? SelfPlayerId;

        private readonly ConnectionManager _connection;
        private readonly SignalBus _signalBus;
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
        private async Task Connect(IPEndPoint endPoint)
        {
            var isConnected = await _connection.Connect(endPoint, 
                new ClientConnectData(clientName));
        }
        private void StartHost()
        {
            ResetInternalState();
            var player = new PlayerInfo(ClientName, startMoney, 0);
            player.PlayerId = (Guid)players.AddPlayer(player)!;
            SelfPlayerId = player.PlayerId;
            _connection.StartHosting(player.PlayerId);
            OnStateChanged();
            OnPlayersUpdated();
        }
        private void ResetInternalState()
        {
            communityCards.Reset();
            players.Reset();
            SelfPlayerId = null;
            dealerIndex = 0;
            currentPlayerIndex = 0;
            isGameHasStartedOnce = false;
            pot = 0;
            gameStage = GameStage.None;
        }
        private async Task Disconnect()
        {
            ResetInternalState();
            await _connection.Disconnect();
            OnStateChanged();
            OnTableStateChanged();
            OnPlayersUpdated();
        }
        private void ApplyGameStateUpdated(GameStateUpdated s)
        {
            communityCards.Cards = s.communityCards;
            dealerIndex = s.dealerIndex;
            currentPlayerIndex = s.currentPlayerIndex;
            pot = s.pot;
            gameStage = s.stage;
            OnTableStateChanged();
        }
        private void ApplyGameStateAll(GameStateAll s)
        {
            communityCards.Cards = s.communityCards;
            RoomName = s.roomName;
            dealerIndex = s.dealerIndex;
            currentPlayerIndex = s.currentPlayerIndex;
            smallBling = s.smallBlind;
            bigBlind = s.bigBlind;
            SelfPlayerId = s.playerId;
            //добавить maxPlayers (не критично, тк пока макс = 6)
            pot = s.pot;
            players.Players = s.players;
            gameStage = s.stage;
            OnTableStateChanged();
            OnPlayersUpdated();
        }
        private int CorrelatePlayerIndex(int playerIndex)
        {
            return (maxPlayers - CorrelationDifference + playerIndex) % maxPlayers;
        }
        private bool CanStartGame()
        {
            return players.Players.Count > 1 && gameStage is GameStage.None && State is ConnectionState.Hosting;
        }
        private async Task StartGame()
        {
            if (!CanStartGame())
                return;
            foreach (PlayerInfo p in players.Players)
            {
                if (p.Money <= 0)
                    p.Status = PlayerStatus.Out;
                else
                    p.Status = PlayerStatus.Active;
            }
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
            await _connection.SendBroadcast(new GameStateUpdated
                (dealerIndex, currentMaxBet, lastRaiseStep,
                pot, communityCards.Cards, gameStage, currentPlayerIndex));
            OnTableStateChanged();
            OnPlayersUpdated();
        }
        private async Task DealCards()
        {
            cardDeck.ResetDeck();
            cardDeck.Shuffle();

            foreach(PlayerInfo p in players.Players)
            {
                PokerCard c1 = cardDeck.GetCard(),
                    c2 = cardDeck.GetCard();
                p.SetHand(c1, c2);
                await _connection.Send(p.PlayerId, 
                    new DealCardsData(new List<PokerCard>()
                    {
                        c1,c2
                    }));
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
        private void HandleCheck(Guid playerId)
        {
        }
        private void HandleCall(Guid playerId)
        {
            var player = players.GetById(playerId);
            if (player is null)
                return;

            if (player.Money < currentMaxBet)
                return;
        }
        private bool IsPlayerMove(Guid playerId)
        {
            return players.GetBySeatIndex(currentPlayerIndex)!.PlayerId == playerId;
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
        private async void StartTimer(Guid playerId)
        {
            _timerCts?.Cancel();
            _timerCts = new CancellationTokenSource();
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(secondsToMove), _timerCts.Token);
                await OnPlayerTimeout(playerId);
            }
            catch{}
        }
        private async Task OnPlayerTimeout(Guid playerId)
        {
            PlayerInfo player = players.GetById(playerId)!;
            if(gameStage is GameStage.PreFlop)
            {
                await DisconnectPlayer(playerId, "Время на ход вышло");
                return;
            }
            //if(Amo) доделать
        }
        private async Task DisconnectPlayer(Guid playerId, string reason = "") 
        {
            players.RemovePlayer(playerId);
            await _connection.Send(playerId, new GameHostingClosed(reason));
            _connection.RemovePlayer(playerId);
            await _connection.SendBroadcast(new GameUpdated(playerId, GameUpdatedType.Disconnected));
            OnPlayersUpdated();
        }
        private int CorrelationDifference => players.Players.Find(x => x.PlayerId ==
        SelfPlayerId)?.SeatIndex ?? -1;
        private void OnPlayerTurn()
        {
            _signalBus.Publish(new PlayerTurnMessage(currentPlayerIndex, currentMaxBet));
        }
        private void OnTableStateChanged()
        {
            _signalBus.Publish(new TableStateChangedMessage(
                communityCards.Cards,
                gameStage, pot, CorrelatePlayerIndex(dealerIndex)));
        }
        private void OnPlayersUpdated()
        {
            if(SelfPlayerId is null)
                _signalBus.Publish(new PlayersUpdatedMessage(new List<PlayerInfo>()));
            else
                _signalBus.Publish(new PlayersUpdatedMessage(
                    players.GetCorrelated((Guid)SelfPlayerId!)));
        }
        private void OnStateChanged()
        {
            _signalBus.Publish(new StateChangedMessage(State));
        }
        private void OnPrivateCardDealt()
        {
            _signalBus.Publish(new PrivateCardDealtMessage(LocalPlayer!.Hand!.Cards));
        }
        private void OnGameResultsOccured()
        {
        }
        private void OnGameChatMessaged(string name, string message)
        {
            _signalBus.Publish(new ChatMessageReceivedMessage(message, name));
        }
        /*
    public record PlayersUpdatedMessage(List<PlayerInfo> players) : BaseMessage;
    public record PrivateCardDealtMessage(List<PokerCard> hand) : BaseMessage;
    public record TableStateChangedMessage(List<PokerCard> communityCards,
        GameStage stage, decimal pot, int dealerIndex) : BaseMessage;
    public record PlayerTurnMessage(int currentPlayerIndex, decimal currentMaxBet) : BaseMessage;
    public record GameResultsOccurred(Dictionary<Guid, HandCards> cards, 
        List<WinnerInfo> winners) : BaseMessage;
    public record StateChangedMessage(Connection.ConnectionState state) */
    }
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
