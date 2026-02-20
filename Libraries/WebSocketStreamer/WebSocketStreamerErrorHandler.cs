using System;
using System.Diagnostics;

namespace WebSocketStreamer
{
    public delegate void ClientClosedHandler(ushort code, string reason);

    public class WebSocketStreamerErrorHandler : IDisposable
    {
        private readonly WebSocketStreamerClient _client;
        public event Action<string> ErrorEvt;

        public WebSocketStreamerErrorHandler(WebSocketStreamerClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _client = client;
            // The error events, we also hook onto close for more details on errors
            _client.Error += OnClientError;
            _client.Closed += OnClientClosed;
        }

        private void OnClientError(Exception ex)
        {
            LogError(true, $"WebSocket error: Reason: {ex.Message}");
        }

        private void OnClientClosed(ushort code, string reason)
        {
            // This is extremely simple, the user can add more of course if they need to
            if (code != 1000)
            {
                LogError(true, $"WebSocket closed unexpectedly: WS Code: {code}, Reason: {reason}");
            }
            else
            {
                LogError(true, $"WebSocket closed normally (1000): Reason: {reason}");
            }
        }

        private void LogError(bool useDefaultLogging, string message)
        {
            if (useDefaultLogging)
            {
                // We'll use Debug.WriteLine for simplicity and because it's really our only option from the add-on
                Debug.WriteLine(message);
            }
            else
            {
                // Trigger an event so the user can implement the event and use their own error handler
                ErrorEvt?.Invoke(message);
            }
        }

        public void Dispose()
        {
            _client.Error -= OnClientError;
            _client.Closed -= OnClientClosed;
        }
    }
}