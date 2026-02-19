using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker.Models
{
    public class PlayerInfo
    {
        public string Name { get; set; }
        public int Money { get; set; }
        public PlayerInfo(string name, int money)
        {
            Name = name;
            Money = money;
        }
    }
}
