using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Poker.Connection
{
    public class TcpConnection : IDisposable
    {
        private CancellationTokenSource _cts;
        private readonly TcpListener _listener;
        public event Action<TcpClient, DataTransferBase> MessageReceived;
        public TcpConnection()
        {
            _cts = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Any, 7777);//fix
            _ = Listen(_cts.Token);
        }
        public async Task SendMove(ClientMove move)
        {

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
                    _ = HandleTcpClient(incomeConnection, token);
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
        private async Task HandleTcpClient(TcpClient client, CancellationToken token)
        {
            var stream = client.GetStream();
            while (token.IsCancellationRequested)
            {
                try
                {
                    using (var ms = new MemoryStream())
                    {
                        var buffer = new byte[4096];
                        int bytesRead;
                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                        {
                            await ms.WriteAsync(buffer, 0, bytesRead);
                        }
                        var messageString = Encoding.Unicode.GetString(ms.ToArray());
                        HandleClientMessage(client, messageString);
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
        private void HandleClientMessage(TcpClient client, string messageString)
        {
            if (!(JsonSerializer.Deserialize<DataTransferBase>(messageString) is DataTransferBase message))
                return;
            MessageReceived?.Invoke(client, message);
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
