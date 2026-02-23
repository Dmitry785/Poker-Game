using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Poker.Models
{
    public class PlayerInfo
    {
        public Guid PlayerId;
        public string Name { get; set; }
        public decimal Money { get; set; }
        [JsonIgnore]
        public HandCards? Hand { get; set; }
        public decimal CurrentBet { get; set; }
        public int SeatIndex { get; set; }
        public PlayerStatus Status = PlayerStatus.Out;
        public PlayerInfo(string name, decimal money, int seatIndex)
        {
            Name = name;
            Money = money;
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
    public class ConnectedPlayerInfo : PlayerInfo//нужно для хоста
    {
        public IPEndPoint ClientEndPoint;
        public ConnectedPlayerInfo(string name, int money, int seatIndex, IPEndPoint client) 
            :base(name, money, seatIndex)
        {
            ClientEndPoint = client;
        }
    }
}
