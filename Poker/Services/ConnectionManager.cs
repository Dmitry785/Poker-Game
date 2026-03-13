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
        private ConnectionState state = ConnectionState.NotConnected;
        public ConnectionState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                StateChanged?.Invoke();
            }
        }
        private readonly IConnection _connection;
        private readonly Dictionary<Guid, IPEndPoint> _connectedPlayers;
        private readonly Dictionary<IPEndPoint, Guid> _addressBook;
        private IPEndPoint? hostEndPoint;
        private TaskCompletionSource<bool>? connectionTcs;
        private Guid? hostPlayerId;
        public event Action<ClientConnectData, IPEndPoint>? ClientConnecting;
        public event Action<Guid, DataTransferBase>? ClientNewMessage;
        public event Action<DataTransferBase>? MessageFromHost;
        public event Action<Guid>? ClientDisconnected;
        public event Action? StateChanged;
        public object CurrentAddress => _connection.CurrentAddress;
        public ConnectionManager(IConnection connection)
        {
            _connection = connection;
            _connectedPlayers = new();
            _addressBook = new();
            _connection.MessageReceived += HandleMessageReceived;
        }
        public async Task<bool> Connect(IPEndPoint hostEndPoint, ClientConnectData connectData)
        {
            try
            {
                connectionTcs = new TaskCompletionSource<bool>();
                State = ConnectionState.Connecting;
                this.hostEndPoint = hostEndPoint;
                bool sendRes = await SendRaw(hostEndPoint, connectData);
                if (!sendRes)
                {
                    State = ConnectionState.NotConnected;
                    return false;
                }
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));

                var completedTask = await Task.WhenAny(
                     connectionTcs.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    State = ConnectionState.NotConnected;
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
        public async Task Disconnect(string reason = "")
        {
            switch (State)
            {
                case ConnectionState.Hosting:
                    await SendBroadcast(new GameHostingClosed(reason));
                    break;
                case ConnectionState.Connected:
                    await SendToHost(new ClientDisconnectData(reason));
                    break;
                case ConnectionState.Connecting:
                    connectionTcs?.TrySetResult(false);
                    break;
            }
            State = ConnectionState.NotConnected;
        }
        public void StartHosting(Guid hostId)
        {
            if (State is not ConnectionState.NotConnected)
                return;
            hostPlayerId = hostId;
            State = ConnectionState.Hosting;
            _connectedPlayers.Clear();
            hostEndPoint = null;
        }
        public async Task<bool> SendToHost(DataTransferBase dto)
        {
            if(State is ConnectionState.Hosting)
            {
                ClientNewMessage?.Invoke((Guid)hostPlayerId!, dto);
                return true;
            }
            if(hostEndPoint is not null)
                return await _connection.Send(hostEndPoint!, Serialize(dto));
            return false;
        }
        public async Task<bool> Send(Guid playerId, DataTransferBase dto)
        {
            if (State is not ConnectionState.Hosting)
                return false;
            if(playerId == hostPlayerId)
            {
                MessageFromHost?.Invoke(dto);
                return true;
            }
            if (!_connectedPlayers!.TryGetValue(playerId, out var endPoint))
                return false;
            return await SendRaw(endPoint, dto);
        }
        public async Task SendBroadcast(DataTransferBase dto)
        {
            foreach(var endPoint in _connectedPlayers.Keys)
            {
                await Send(endPoint, dto);
            }
            await Send((Guid)hostPlayerId!, dto);
        }
        public void RegisterPlayer(Guid playerId, IPEndPoint endPoint)
        {
            _connectedPlayers[playerId] = endPoint;
            _addressBook[endPoint] = playerId;
        }
        public void RemovePlayer(Guid playerId)
        {
            if (!_connectedPlayers.TryGetValue(playerId, out var endPoint))
                return;
            _connectedPlayers.Remove(playerId);
            _addressBook.Remove(endPoint);
        }
        public async Task<bool> SendRaw(IPEndPoint endPoint, DataTransferBase dto)
        {
            return await _connection.Send(endPoint, Serialize(dto));
        }
        private void HandleMessageReceived(IPEndPoint endPoint, byte[] data)
        {
            if (!(JsonSerializer.Deserialize<DataTransferBase>(data) is DataTransferBase message))
                return;
            switch (State)
            {
                case ConnectionState.NotConnected:
                    HandleMessage_NotConnected(endPoint, message);
                    break;
                case ConnectionState.Connected:
                    HandleMessage_Connected(endPoint, message);
                    break;
                case ConnectionState.Hosting:
                    HandleMessage_Hosting(endPoint, message);
                    break;
                case ConnectionState.Connecting:
                    HandleMessage_Connecting(endPoint, message);
                    break;
            }
        }
        private void HandleMessage_NotConnected(IPEndPoint endPoint, DataTransferBase message)
        {

        }
        private void HandleMessage_Connecting(IPEndPoint endPoint, DataTransferBase message)
        {
            if (endPoint.ToString() != hostEndPoint!.ToString())
                return;
            switch (message)
            {
                case GameStateAll c:
                    State = ConnectionState.Connected;
                    connectionTcs?.SetResult(true);
                    MessageFromHost?.Invoke(c);
                    break;
                case ConnectionDeclined c:
                    State = ConnectionState.NotConnected;
                    connectionTcs?.SetResult(false);
                    MessageFromHost?.Invoke(c);
                    break;
            }
        }
        private void HandleMessage_Connected(IPEndPoint endPoint, DataTransferBase message)
        {
            if(endPoint.ToString()!=hostEndPoint!.ToString())
            {
                return;
            }
            MessageFromHost?.Invoke(message);
        }
        private void HandleMessage_Hosting(IPEndPoint endPoint, DataTransferBase message)
        {
            if (!_addressBook.TryGetValue(endPoint, out var playerId))
            {
                if (message is not ClientConnectData c)
                    return;
                ClientConnecting?.Invoke(c, endPoint);
            }
            else
            {
                ClientNewMessage?.Invoke(playerId, message);
            }
        }
        private byte[] Serialize(DataTransferBase dto)
        {
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));
        }
    }
}
