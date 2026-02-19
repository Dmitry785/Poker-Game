using Poker.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Poker.Services
{
    public class ConnectionCommandHandler
    {
        private readonly TcpConnection _connection;
        public ConnectionCommandHandler(TcpConnection connection)
        {
            _connection = connection;
        }
        
    }
}
