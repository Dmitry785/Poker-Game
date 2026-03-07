using Poker.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Poker.Services
{
    public class ConnectionManager
    {
        private readonly IConnection _connection;
        public event Action<IPEndPoint, DataTransferBase>? MessageReceived;
        public object CurrentAddress => _connection.CurrentAddress;
        public ConnectionManager(IConnection connection)
        {
            _connection = connection;
            _connection.MessageReceived += HandleMessageReceiver;
        }
        public async Task<bool> Send(IPEndPoint endPoint, DataTransferBase dto)
        {
            return await _connection.Send(endPoint, Serialize(dto));
        }
        private void HandleMessageReceiver(IPEndPoint endPoint, byte[] data)
        {
            if (!(JsonSerializer.Deserialize<DataTransferBase>(data) is DataTransferBase message))
                return;
            MessageReceived?.Invoke(endPoint, message);
        }
        private byte[] Serialize(DataTransferBase dto)
        {
            return Encoding.Unicode.GetBytes(JsonSerializer.Serialize(dto));
        }
    }
}
