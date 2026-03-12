using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace Poker.Models
{
    //[JsonDerivedType(typeof(PlayerInfo), typeDiscriminator:"playerInfo")]
    //[JsonDerivedType(typeof(ConnectedPlayerInfo), typeDiscriminator:"connectedPlayerInfo")]
    public class PlayerInfo
    {
        public Guid PlayerId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Money { get; set; }
        [JsonIgnore]
        public HandCards? Hand { get; set; }
        public decimal CurrentBet { get; set; }
        public int SeatIndex { get; set; }
        public PlayerStatus Status { get; set; } = PlayerStatus.Out;
        public PlayerInfo()
        {

        }
        public PlayerInfo(string name, decimal money, int seatIndex = 0)
        {
            Name = name;
            Money = money;
        }
        public void SetHand(List<PokerCard> cards)
        {
            if (cards.Count < 2)
                return;
            Hand = new HandCards(cards[0], cards[1]);
        }
        public void SetHand(PokerCard card1, PokerCard card2)
        {
            Hand = new HandCards(card1, card2);
        }
    }
    public enum PlayerStatus
    {
        Active,
        AllIn,
        Folded,
        Out
    }
    public class PlayerListManager
    {
        private List<PlayerInfo> players = new();
        public int MaxPlayers;
        public PlayerListManager(int maxPlayers)
        {
            MaxPlayers = maxPlayers;
        }
        public List<PlayerInfo> Players
        {
            get => players;
            set
            {
                Reset();
                foreach(var player in value)
                {
                    if (!CanAddPlayer(player))
                        break;
                    AddPlayerWithCorrelationAndNameFix(player);
                }
            }
        }
        public bool CanAddPlayer(PlayerInfo player)
        {
            return players.Count <= MaxPlayers;
        }
        public void RemovePlayer(Guid playerId)
        {
            players.RemoveAll(x => x.PlayerId == playerId);
        }
        public PlayerInfo? GetBySeatIndex(int seatIndex)
        {
            return players.FirstOrDefault(x => x.SeatIndex == seatIndex);
        }
        public PlayerInfo? GetById(Guid playerId)
        {
            return players.FirstOrDefault(x => x.PlayerId == playerId);
        }
        public Guid? AddPlayer(PlayerInfo player)
        {
            if (!CanAddPlayer(player))
                return null;
            player.PlayerId = Guid.NewGuid();
            AddPlayerWithCorrelationAndNameFix(player);
            return player.PlayerId;
        }
        private void AddPlayerWithCorrelationAndNameFix(PlayerInfo player)
        {
            if(players.Exists(x => x.Name == player.Name))
            {
                bool isNameRight = false;
                for(int nameId = 1; nameId < _selectNameIdAttempts; nameId++)
                {
                    var nameIdPostfix = $" ({nameId})";
                    if (!players.Exists(x => x.Name == player.Name + nameIdPostfix))
                    {
                        player.Name += nameIdPostfix;
                        isNameRight = true;
                        break;
                    }
                }
                if (!isNameRight)
                {
                    //можно добавить префикс
                }
            }
            if (!players.Exists(x => x.SeatIndex == player.SeatIndex))
            {
                players.Add(player);
                return;
            }
            for (int i = 0; i < MaxPlayers; i++)
            {
                if (players.Exists(x => x.SeatIndex == i))
                    continue;
                player.SeatIndex = i;
                players.Add(player);
                return;
            }
        }
        public void Reset()
        {
            players.Clear();
        }
        public List<PlayerInfo> GetCorrelated(Guid playerId)
        {
            var player = players.Find(x => x.PlayerId == playerId);
            if (player is null)
                throw new Exception();
            int seatIndex = player.SeatIndex;
            var res = players.ToList();
            for(int i = 0; i < res.Count; i++)
            {
                var idx = res[i].SeatIndex - seatIndex;
                if (idx < 0)
                    idx = MaxPlayers + idx;
                res[i].SeatIndex = idx;
            }
            return res;
        }
        public void ChangedPlayerName(string playerName, string newName)
        {
            var player = players.FirstOrDefault(x => x.Name == playerName);
            if (player is not null)
                player.Name = newName;
        }
        private readonly int _selectNameIdAttempts = 10;
    }
    public class WinnerInfo
    {
        public Guid PlayerId { get; set; }
        public decimal Amount { get; set; }
        public string HandTitle { get; set; }
        public WinnerInfo(Guid playerId, decimal amount, string handTitle)
        {
            PlayerId = playerId;
            Amount = amount;
            HandTitle = handTitle;
        }
    }
}
