using System;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace WebSocketStreamer.Networking
{
    public class WebSocketStreamerReceive
    {
        public event Action<string> TextMessageReceived;
        public event Action<byte[]> BinaryMessageReceived;

        public WebSocketStreamerReceive(MessageWebSocket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");
            socket.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            DataReader reader = args.GetDataReader();

            if (args.MessageType == SocketMessageType.Binary)
            {
                var bytes = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(bytes);
                BinaryMessageReceived?.Invoke(bytes);
            }
            else
            {
                reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                string message = reader.ReadString(reader.UnconsumedBufferLength);
                TextMessageReceived?.Invoke(message);
            }
        }
    }
}