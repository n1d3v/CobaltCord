using System;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace WebSocketStreamer.Networking
{
    public class WebSocketStreamerReceive
    {
        public event Action<string> TextMessageReceived;

        public WebSocketStreamerReceive(MessageWebSocket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            socket.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            DataReader reader = args.GetDataReader();
            reader.UnicodeEncoding = UnicodeEncoding.Utf8;

            string message = reader.ReadString(reader.UnconsumedBufferLength);

            TextMessageReceived?.Invoke(message);
        }
    }
}