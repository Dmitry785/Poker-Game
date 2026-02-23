using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Poker.Connection
{
    public partial class TcpConnection : IDisposable
    {
        private CancellationTokenSource _cts;
        private readonly TcpListener _listener;
        private readonly Dictionary<IPEndPoint, TcpClient> _activeClients = new();
        private readonly int _sendAttempts = 3;
        public event Action<IPEndPoint, DataTransferBase>? MessageReceived;
        public IPEndPoint CurrentIP
        {
            get {
                foreach(IPAddress address in Dns.GetHostAddresses(Dns.GetHostName(), AddressFamily.InterNetwork))
                {
                    return new IPEndPoint(address, Port);
                }
                throw new Exception();
            }
        }
        public int Port
        {
            get
            {
                return ((IPEndPoint)_listener.LocalEndpoint).Port;
            }
        }
        public TcpConnection()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, 0);
            _ = Listen(_cts.Token);
        }
        public async Task<bool> Send(IPEndPoint clientIP, DataTransferBase message)
        {
            if(!_activeClients.TryGetValue(clientIP, out var client) || !client.Connected)
            {
                client = new TcpClient();
                await client.ConnectAsync(clientIP);
                _activeClients[clientIP] = client;
                _ = HandleTcpClient(client, clientIP, _cts.Token);
            }
            byte[] data = Serialize(message);
            byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
            var stream = client.GetStream();
            await stream.WriteAsync(lengthPrefix, 0, 4);
            await stream.WriteAsync(data, 0, data.Length);
            return true;
        }
        private async Task Listen(CancellationToken token)
        {
            try
            {
                _listener.Start();
                while (!token.IsCancellationRequested)
                {
                    TcpClient incomeConnection = default!;
                    try
                    {
                        incomeConnection = await _listener.AcceptTcpClientAsync(token);
                    }
                    catch
                    {
                        continue;
                    }
                    IPEndPoint endPoint = (IPEndPoint)incomeConnection.Client.RemoteEndPoint!;
                    _activeClients[endPoint] = incomeConnection;
                    _ = HandleTcpClient(incomeConnection, endPoint, token);
                }
            }
            catch
            {

            }
            finally
            {
                _listener.Stop();
            }
        }
        private async Task HandleTcpClient(TcpClient client, IPEndPoint endPoint, CancellationToken token)
        {
            using (client)
            using (var stream = client.GetStream())
            try
            {
                while (!token.IsCancellationRequested)
                {
                    using (CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token))
                    {
                        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));

                        byte[] lengthBuffer = new byte[4];
                        await stream.ReadExactlyAsync(lengthBuffer, 0, 4, timeoutCts.Token);
                        int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                        if (messageLength <= 0)
                            continue;
                        byte[] messageBuffer = new byte[messageLength];
                        await stream.ReadExactlyAsync(messageBuffer, 0, messageLength, token);
                        var messageString = Encoding.Unicode.GetString(messageBuffer);
                        HandleClientMessage(endPoint, messageString);
                    }
                }
            }
            catch
            {
                    _activeClients.Remove(endPoint);
                    client.Dispose();
            }
        }
        private void HandleClientMessage(IPEndPoint endPoint, string messageString)
        {
            if (!(JsonSerializer.Deserialize<DataTransferBase>(messageString) is DataTransferBase message))
                return;
            MessageReceived?.Invoke(endPoint, message);
        }
        private byte[] Serialize(DataTransferBase dto)
        {
            return Encoding.Unicode.GetBytes(JsonSerializer.Serialize(dto));
        }
        public void Dispose()
        {
            _cts.Cancel();
        }
    }
    public enum ConnectionState
    {
        NotConnected,
        Connected,
        Connecting,
        Hosting
    }
}
