using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace WebSocketStreamer.Networking
{
    public class WebSocketStreamerSend : IDisposable
    {
        private readonly DataWriter _writer;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public WebSocketStreamerSend(MessageWebSocket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            _writer = new DataWriter(socket.OutputStream);
        }

        public async Task SendText(string message)
        {
            await _lock.WaitAsync();

            try
            {
                _writer.WriteString(message);
                await _writer.StoreAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Dispose()
        {
            _writer.DetachStream();
            _writer.Dispose();
            _lock.Dispose();
        }
    }
}