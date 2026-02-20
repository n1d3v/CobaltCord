using System;

namespace WebSocketStreamer
{
    public class WebSocketStreamerInit
    {
        private WebSocketStreamerSettings wsSettings;
        private static bool _initialized;

        public void Initialize(string wsUrl)
        {
            if (_initialized)
                return;

            wsSettings = new WebSocketStreamerSettings();

            DefaultReceiveBufferSize = wsSettings.BufferSize;
            DefaultSendBufferSize = wsSettings.BufferSize;
            wsSettings.WebSocketURL = wsUrl;

            _initialized = true;
        }

        public WebSocketStreamerClient CreateClient()
        {
            if (!_initialized)
                throw new InvalidOperationException("WebSocketStreamerInit has not been initialized.");

            return new WebSocketStreamerClient(wsSettings.WebSocketURL);
        }

        // You can still set your own buffer size if you'd like.
        public static int DefaultReceiveBufferSize { get; private set; }
        public static int DefaultSendBufferSize { get; private set; }
    }
}