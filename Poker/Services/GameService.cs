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
            get => _connection.State;
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
            OnPlayerUpsert(connectingClient);
        }
        private void HandleClientNewMessage(Guid playerId, DataTransferBase message)
        {
            PlayerInfo p = players.Players.First(x => x.PlayerId == playerId);
            switch (message)
            {
                case ClientMove c:
                    _timerCts?.Cancel();
                    break;
            }
        }
        private void HandleMessageFromHost(DataTransferBase message)
        {
            switch (message)
            {
                case DealCardsData c:
                    LocalPlayer!.SetHand(c.hand);
                    _signalBus.Publish(new PrivateCardDealtMessage(c.hand));
                    break;
            }
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
                    OnPlayerUpsert(LocalPlayer);
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
            if(await HandleSystemCommand(command))
                return;
            if (State is ConnectionState.NotConnected)
                return;
            switch (command)
            {
                case BetRaiseCommand c:
                    break;
                case CallCommand c:
                    break;
                case FoldCommand c:
                    break;
                case CheckCommand c:
                    break;
                case StartGameCommand c:
                    break;
            }
            /*ublic abstract record GameCommand;
        public record CallCommand : GameCommand;
        public record FoldCommand : GameCommand;
        public record CheckCommand : GameCommand;
        public record BetRaiseCommand(decimal amount) : GameCommand;
        public record StartHostCommand : GameCommand;
        public record StartGameCommand : GameCommand;
        public record ConnectCommand(IPEndPoint hostEndPoint) : GameCommand;
        public record DisconnectCommand(string reason = "") : GameCommand;*/
    }
        private async Task<bool> HandleSystemCommand(GameCommand command)
        {
            switch (command)
            {
                case DisconnectCommand c:
                    if (State is ConnectionState.NotConnected)
                        return true;
                    await OnDisconnected();
                    return true;
                case ConnectCommand c:
                    if (State is not ConnectionState.NotConnected)
                        return true;
                    await OnConnect(c.hostEndPoint);
                    return true;
                case StartHostCommand c:
                    if (State is not ConnectionState.NotConnected)
                        return true;
                    OnStartHosting();
                    return true;
            }
            return false;
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
        private async Task OnConnect(IPEndPoint endPoint)
        {
            var isConnected = await _connection.Connect(endPoint, 
                new ClientConnectData(clientName));
        }
        private void OnStartHosting()
        {
            ResetInternalState();
            var player = new PlayerInfo(ClientName, startMoney, 0);
            player.PlayerId = (Guid)players.AddPlayer(player)!;
            SelfPlayerId = player.PlayerId;
            _connection.StartHosting(player.PlayerId);
            OnStateChanged();
            OnPlayerUpsert(player);
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
        private async Task OnDisconnected()
        {
            ResetInternalState();
            await _connection.Disconnect();
            OnStateChanged();
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
        private void CorrelatePlayerIndex(ref int playerIndex, int difference)
        {
            playerIndex = (maxPlayers - difference + playerIndex) % maxPlayers;
        }
        private int CorrelatePlayerIndex(int playerIndex, int difference)
        {
            return (maxPlayers - difference + playerIndex) % maxPlayers;
        }
        private async Task OnGameStarted()
        {
            if (players.Players.Count <= 1 || gameStage != GameStage.None || State is not ConnectionState.Hosting)
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
            OnPlayerdLeft(playerId);
        }
        private int CorrelationDifference => players.Players.Find(x => x.PlayerId == SelfPlayerId)!.SeatIndex;
        private void OnTableStateChanged()
        {
            int correlatedDealerIndex = CorrelatePlayerIndex(dealerIndex, CorrelationDifference);
            int correlatedCurrentPlayerIndex = CorrelatePlayerIndex(currentPlayerIndex, CorrelationDifference);
            _signalBus.Publish(new TableStateChangedMessage(correlatedDealerIndex,
                communityCards.Cards, gameStage));
        }
        private void OnPlayersUpdated()
        {
            _signalBus.Publish(new PlayersUpdatedMessage(
                players.GetCorrelated((Guid)SelfPlayerId!)));
        }
        private void OnPlayerUpsert(PlayerInfo player)
        {
            _signalBus.Publish(new PlayerUpdatedMessage(player));
        }
        private void OnPlayerdLeft(Guid id)
        {
            _signalBus.Publish(new PlayerLeftMessage(id));
        }
        private void OnStateChanged()
        {
            _signalBus.Publish(new StateChangedMessage());
        }
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
