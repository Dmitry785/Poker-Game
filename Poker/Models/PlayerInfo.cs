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
        public string Name { get; set; }
        public int Money { get; set; }
        [JsonIgnore]
        public PokerCard[]? Hand { get; set; }
        public PlayerInfo(string name, int money)
        {
            Name = name;
            Money = money;
        }
    }
    public class ConnectedPlayerInfo : PlayerInfo//нужно для хоста
    {
        public IPEndPoint ClientEndPoint;
        //карты пользователя
        public ConnectedPlayerInfo(string name, int money, IPEndPoint client) 
            :base(name, money)
        {
            ClientEndPoint = client;
        }
    }
}
