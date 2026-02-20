using System;
using System.Threading.Tasks;

namespace WebSocketStreamer.Networking
{
    public class WebSocketStreamerReconnection
    {
        private readonly WebSocketStreamerClient _client;
        private readonly int _maxRetries;
        private readonly int _baseDelayMs;

        public event Action Reconnected;
        public event Action Failed;

        public WebSocketStreamerReconnection(WebSocketStreamerClient client, int maxRetries = 5, int baseDelayMs = 2000)
        {
            if (client == null)
                throw new ArgumentNullException("You are missing the WebSocket client, please attach it to the function.");

            _client = client;
            _maxRetries = maxRetries;
            _baseDelayMs = baseDelayMs;

            _client.Closed += OnClosed;
        }

        private async void OnClosed(ushort code, string reason)
        {
            await AttemptReconnect();
        }

        public async Task AttemptReconnect()
        {
            int retries = 0;

            while (!_client.IsConnected && retries < _maxRetries)
            {
                retries++;
                try
                {
                    await Task.Delay(_baseDelayMs * retries);
                    await _client.Connect();
                }
                catch
                {
                    // Ignore errors, keep trying over and over again...
                }
            }

            if (_client.IsConnected)
                Reconnected?.Invoke();
            else
                Failed?.Invoke();
        }
    }
}