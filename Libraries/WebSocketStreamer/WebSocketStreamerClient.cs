using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace WebSocketStreamer
{
    public class WebSocketStreamerClient : IDisposable
    {
        private MessageWebSocket _socket;
        public MessageWebSocket Socket => _socket;

        public event Action<string> MessageReceived;
        public event Action<ushort, string> Closed;
        public event Action<Exception> Error;

        private readonly string _url;
        public bool IsConnected { get; private set; }
        private readonly Dictionary<string, string> _headers = new Dictionary<string, string>();

        public event ClientClosedHandler OnClientClosedEvent;

        public WebSocketStreamerClient(string url)
        {
            _url = url;
            _socket = new MessageWebSocket();
            _socket.Control.MessageType = SocketMessageType.Utf8;

            _socket.MessageReceived += (sender, args) =>
            {
                var reader = args.GetDataReader();
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;

                var message = reader.ReadString(reader.UnconsumedBufferLength);
                MessageReceived?.Invoke(message);
            };

            _socket.Closed += (sender, args) =>
            {
                Closed?.Invoke(args.Code, args.Reason);
                OnClientClosedEvent?.Invoke(args.Code, args.Reason);
            };
        }

        public void AddHeader(string name, string value)
        {
            _headers[name] = value;
        }

        public async Task Connect()
        {
            foreach (var kvp in _headers)
                _socket.SetRequestHeader(kvp.Key, kvp.Value);

            await _socket.ConnectAsync(new Uri(_url));
            IsConnected = true;
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _socket = null;
        }
    }
}