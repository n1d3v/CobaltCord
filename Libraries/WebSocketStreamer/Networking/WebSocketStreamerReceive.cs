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

            if (sender.Control.MessageType == SocketMessageType.Utf8)
            {
                reader.UnicodeEncoding = UnicodeEncoding.Utf8;
                string message = reader.ReadString(reader.UnconsumedBufferLength);
                TextMessageReceived?.Invoke(message);
            }
            else
            {
                byte[] data = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(data);
                BinaryMessageReceived?.Invoke(data);
            }
        }
    }
}