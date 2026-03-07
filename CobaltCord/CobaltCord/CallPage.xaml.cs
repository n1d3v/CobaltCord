using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CobaltCord.Networking;
using CobaltCord.Classes;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Windows.UI.Core;
using Newtonsoft.Json.Linq;
using Windows.Phone.UI.Input;
using WebSocketStreamer;
using WebSocketStreamer.Networking;

namespace CobaltCord
{
    public sealed partial class CallPage : Page
    {
        private string userId;
        private string channelId;
        private string dscToken;
        private int heartbeatInterval;
        private bool _isConnected = false;

        internal static readonly API api = API.Instance;
        private WebSocket _webSocket;

        private WebSocketStreamerClient _client;
        private WebSocketStreamerSend _sender;
        private WebSocketStreamerErrorHandler _errorHandler;

        public CallPage()
        {
            this.InitializeComponent();
            _webSocket = WebSocket.Instance;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        private async Task RingConversation()
        {
            await Task.Delay(3000);
            var ringPayload = new { recipients = (string[])null };
            await api.SendAPI($"channels/{channelId}/call/ring", HttpMethod.Post, dscToken, ringPayload, null, null, null);

            var payload = JsonConvert.SerializeObject(new
            {
                op = 4,
                d = new
                {
                    guild_id = (string)null, // This is for DMs for now, servers / guilds come later
                    channel_id = channelId,
                    self_mute = true,
                    self_deaf = false,
                    self_video = false,
                    flags = 2
                }
            });

            await _webSocket.Sender.SendText(payload);
            DebugLabel.Text = "Started ringing the user!\nAttempting to initialize receiving voice...";

            await InitializeVoice();
        }

        private async Task InitializeVoice()
        {
            // Wait for the voice token and endpoint to be initialized before accessing the data
            await WaitForVoiceReady();

            _client = new WebSocketStreamerClient($"wss://{_webSocket.voiceEndpoint}/?v=9");
            _client.MessageReceived += HandleMessageData;

            await _client.Connect();
            _isConnected = true;

            DebugLabel.Text = "Connected to a call successfully!\nSending the initial voice payload to Discord...";

            _errorHandler = new WebSocketStreamerErrorHandler(_client);
            _sender = new WebSocketStreamerSend(_client.Socket);

            var payload = JsonConvert.SerializeObject(new
            {
                op = 0,
                d = new
                {
                    server_id = channelId,
                    channel_id = channelId,
                    user_id = userId,
                    session_id = GenerateSessionId(),
                    token = _webSocket.voiceToken,
                    max_dave_protocol_version = 1,
                    self_mute = true,
                    self_deaf = false,
                    self_video = false,
                    flags = 2,
                    video = true,
                    streams = new[]
                    {
                        new { type = "video", rid = "100", quality = 100 },
                        new { type = "video", rid = "50", quality = 50 }
                    }
                }
            });

            await _webSocket.Sender.SendText(payload);
            DebugLabel.Text = "Connected to a call successfully!\nSent the initial voice payload to Discord!";
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var data = e.Parameter as HelperClasses.CallNavData;
            if (data != null)
            {
                // Set the channel ID for the call UI
                userId = data.UserId;
                channelId = data.ChannelId;
                dscToken = SettingsMgr.DiscordTkn;

                try
                {
                    await RingConversation();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An exception occurred! {ex.Message}");
                }
            }
        }

        private async Task SendHeartbeat()
        {
            while (_isConnected)
            {
                var heartbeatPayload = JsonConvert.SerializeObject(new
                {
                    op = 3,
                    d = new
                    {
                        t = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds, // This is a guess, but that is most likely what it is.
                        seq_ack = 1
                    }
                });

                try
                {
                    await _sender.SendText(heartbeatPayload);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error sending heartbeat: {ex.Message}");
                }

                await Task.Delay(heartbeatInterval);
            }
        }

        private async Task WaitForVoiceReady()
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler handler = null;
            handler = (s, e) =>
            {
                _webSocket.VoiceReady -= handler;
                tcs.TrySetResult(true);
            };

            _webSocket.VoiceReady += handler;

            await tcs.Task;
        }

        // Generates something similar to a real voice session ID on Discord
        string GenerateSessionId()
        {
            var bytes = new byte[16];
            new Random().NextBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private void HandleMessageData(string data)
        {
            var json = JsonConvert.DeserializeObject<JObject>(data);
            int opCode = json["op"] != null ? (int)json["op"] : -1;

            switch (opCode)
            {
                case 0:
                    string eventType = json["t"] != null ? (string)json["t"] : "";
                    switch (eventType)
                    {
                        default:
                            // Only uncomment if you want to debug an event from Discord, this is a mess in the console.
                            Debug.WriteLine($"[WS-VOICE] Unhandled event: {eventType}, data: {json["d"]?.ToString(Formatting.None)}");
                            break;
                    }
                    break;
                case 8:
                    heartbeatInterval = json["d"]?["heartbeat_interval"] != null
                        ? (int)json["d"]["heartbeat_interval"]
                        : 0;
                    Task.Run(() => SendHeartbeat());
                    break;
                default:
                    Debug.WriteLine($"Unknown op code: {opCode}, data: {data}");
                    break;
            }
        }

        private async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            if (Frame.CanGoBack)
            {
                var payload = JsonConvert.SerializeObject(new
                {
                    op = 4,
                    d = new
                    {
                        guild_id = (string)null, // This is for DMs for now, servers / guilds come later
                        channel_id = (string)null,
                        self_mute = true,
                        self_deaf = false,
                        self_video = false,
                        flags = 2
                    }
                });

                await _webSocket.Sender.SendText(payload);
                Frame.GoBack();
            }
            else
            {
                Debug.WriteLine("No pages to go back to.");
            }
        }
    }
}
