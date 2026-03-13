using Poker.Services;
using System;
using System.Collections.Concurrent;
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
    public interface IConnection
    {
        Task<bool> Send(IPEndPoint clientIP, byte[] message);
        event Action<IPEndPoint, byte[]>? MessageReceived;
        object CurrentAddress { get; }
    }
    public partial class TcpConnection : IDisposable, IConnection
    {
        private CancellationTokenSource _cts;
        private readonly TcpListener _listener;
        private readonly Dictionary<IPEndPoint, TcpClient> _activeClients = new();
        private ConcurrentDictionary<IPEndPoint, SemaphoreSlim> _sendLocks = new();
        public event Action<IPEndPoint, byte[]>? MessageReceived;
        public ILogger Logger = new NullLogger();
        public IPEndPoint CurrentIP
        {
            get {
                foreach(IPAddress address in Dns.GetHostAddresses(Dns.GetHostName(), AddressFamily.InterNetwork))
                {
                    return new IPEndPoint(address, Port);
                }
                return new IPEndPoint(IPAddress.Loopback, Port);
            }
        }
        public int Port
        {
            get
            {
                return ((IPEndPoint)_listener.LocalEndpoint).Port;
            }
        }

        public object CurrentAddress => CurrentIP;

        public TcpConnection()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, 0);
            _ = Listen(_cts.Token);
        }
        public async Task<bool> Send(IPEndPoint clientIP, byte[] data)
        {
            Logger.Message($"Начата отправка {clientIP}...");
            var semaphore = _sendLocks.GetOrAdd(clientIP, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                if (!_activeClients.TryGetValue(clientIP, out var client) || !client.Connected)
                {
                    Logger.Message($"Требуется создать TCP {clientIP}...");
                    client?.Dispose();
                    client = new TcpClient();
                    await client.ConnectAsync(clientIP);
                    Logger.Message($"TCP установлено {clientIP}");
                    _activeClients[clientIP] = client;
                    _ = HandleTcpClient(client, clientIP, _cts.Token);
                }
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(lengthPrefix, 0, 4);
                await stream.WriteAsync(data, 0, data.Length);
                await stream.FlushAsync();
                Logger.Message($"Отправка успешно выполнена {clientIP}");
                return true;
            }
            catch(Exception ex)
            {
                Logger.Error($"Отправка не вополнена {clientIP}", ex.Message);
                _activeClients.Remove(clientIP, out var client);
                client?.Dispose();
                return false;
            }
            finally
            {
                Logger.Message($"Отправка закончена");
                semaphore.Release();
            }
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
                    Logger.Message($"Входящее TCP {endPoint}");
                    _activeClients[endPoint] = incomeConnection;
                    _ = HandleTcpClient(incomeConnection, endPoint, token);
                }
            }
            catch
            {
            }
            finally
            {
                Logger.Message($"Чтение завершено");
                _listener.Stop();
            }
        }
        private async Task HandleTcpClient(TcpClient client, IPEndPoint endPoint, CancellationToken token)
        {
            Logger.Message($"Обработка TCP {endPoint}");
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    while (!token.IsCancellationRequested)
                    {
                        byte[] lengthBuffer = new byte[4];
                        int bytesRead = await stream.ReadAsync(lengthBuffer, 0, 4, token);
                        if (bytesRead == 0) break;

                        int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                        byte[] messageBuffer = new byte[messageLength];
                        await stream.ReadExactlyAsync(messageBuffer, 0, messageLength, token);
                        HandleClientMessage(endPoint, messageBuffer);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Error($"Ошибка при обработке {endPoint}", ex.Message);
            }
            finally
            {
                Logger.Message($"Обработка завершена {endPoint}");
                _activeClients.Remove(endPoint);
                _sendLocks.TryRemove(endPoint, out _);
            }
        }
        private void HandleClientMessage(IPEndPoint endPoint, byte[] message)
        {
            try
            {
                MessageReceived?.Invoke(endPoint, message);
            }
            catch(Exception ex)
            {
                Logger.Error($"Ошибка в сообщении {endPoint}", ex.Message);
            }
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
