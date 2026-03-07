using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketStreamer;
using WebSocketStreamer.Networking;
using CobaltCord.Classes;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System.IO;

namespace CobaltCord.Networking
{
    public class WebSocket
    {
        // This allows us to use one single WebSocket instance throughout the entire client
        private static readonly Lazy<WebSocket> _instance = new Lazy<WebSocket>(() => new WebSocket());
        public static WebSocket Instance => _instance.Value;

        // WebSocketStreamer properties
        private WebSocketStreamerClient _client;
        private WebSocketStreamerSend _sender;
        public WebSocketStreamerSend Sender => _sender;

        // SharpZipLib properties
        private Inflater _inflater = new Inflater();
        private MemoryStream _inflateBuffer = new MemoryStream();

        // Discord WS properties
        private string gatewayUrl = "wss://gateway.discord.gg/?v=9&encoding=json&compress=zlib-stream";
        private string dscToken;

        // WebSocket properties
        private bool _isConnected = false;
        private int heartbeatInterval;

        // Voice data properties
        public string voiceToken;
        public string voiceEndpoint;

        // WebSocket events
        public event EventHandler VoiceReady;
        public event EventHandler ReadyReceived;

        // Channel data
        public JArray recipientsData;
        public JArray privateChannelsData;

        private WebSocket()
        {
            dscToken = SettingsMgr.DiscordTkn;
        }

        public async Task StartSocket()
        {
            try
            {
                _client = new WebSocketStreamerClient(gatewayUrl);
                _client.BinaryMessageReceived += HandleMessage;

                await _client.Connect();
                _sender = new WebSocketStreamerSend(_client.Socket);

                _isConnected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing WebSocket: {ex.GetType()} - {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private async Task SendIdentify()
        {
            var config = new ConfigMgr();
            var identifyPayload = new
            {
                op = 2,
                d = new
                {
                    token = dscToken,
                    properties = new
                    {
                        os = config.OperatingSystem,
                        browser = config.BrowserName,
                        device = string.Empty,
                        system_locale = config.SystemLocale,
                        has_client_mods = config.HasClientMods,
                        browser_user_agent = config.BrowserUA,
                        browser_version = config.BrowserVer,
                        os_version = config.OSVersion,
                        referrer = config.DCReferrer,
                        referring_domain = config.DCReferringDomain,
                        referrer_current = config.DCReferringCurrent,
                        referring_domain_current = config.DCReferringCurrentDomain,
                        release_channel = config.DCClientState,
                        client_event_source = config.DCClientEvtSrc,
                        client_launch_id = config.ClientLaunchId,
                        is_fast_connect = true
                    }
                },
                client_state = new { guild_versions = new { } }
            };

            string jsonPayload = JsonConvert.SerializeObject(identifyPayload);
            await _sender.SendText(jsonPayload);
        }

        private async Task SendHeartbeat()
        {
            while (_isConnected)
            {
                var heartbeatPayload = new { op = 1, d = (object)null };
                string jsonPayload = JsonConvert.SerializeObject(heartbeatPayload);

                try
                {
                    await _sender.SendText(jsonPayload);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error sending heartbeat: {ex.Message}");
                }

                await Task.Delay(heartbeatInterval);
            }
        }

        private void HandleVoiceServerUpdate(JToken data)
        {
            try
            {
                voiceToken = data["token"]?.ToString();
                voiceEndpoint = data["endpoint"]?.ToString();

                Debug.WriteLine($"Voice token: {voiceToken}");
                Debug.WriteLine($"Voice WSS endpoint: {voiceEndpoint}");

                VoiceReady?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling VOICE_SERVER_UPDATE: {ex}");
            }
        }

        private async Task HandleReadyEvt(string data)
        {
            var parsedReady = JObject.Parse(data);
            await UserStatusMgr.HandleUserStatus(parsedReady);
            ReadyReceived?.Invoke(this, EventArgs.Empty);
        }

        private async void HandleMessage(byte[] data)
        {
            try
            {
                _inflateBuffer.Write(data, 0, data.Length);
                byte[] bufferArray = _inflateBuffer.ToArray();

                if (!EndsWithFlushSuffix(bufferArray))
                    return;

                _inflater.SetInput(bufferArray);
                using (var output = new MemoryStream())
                {
                    byte[] buf = new byte[4096];
                    int read;
                    while ((read = _inflater.Inflate(buf)) > 0)
                        output.Write(buf, 0, read);

                    byte[] decompressed = output.ToArray();
                    string jsonString = System.Text.Encoding.UTF8.GetString(decompressed, 0, decompressed.Length);

                    _inflateBuffer.SetLength(0);
                    await HandleMessageData(jsonString);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error decoding zlib message: {ex}");
                _inflateBuffer.SetLength(0);
            }
        }

        private async Task HandleMessageData(string data)
        {
            var json = JsonConvert.DeserializeObject<JObject>(data);
            int opCode = json["op"] != null ? (int)json["op"] : -1;

            switch (opCode)
            {
                case 0:
                    string eventType = json["t"] != null ? (string)json["t"] : "";
                    switch (eventType)
                    {
                        case "READY":
                            string wsJsonEvt = json["d"].ToString(Formatting.None);
                            recipientsData = (JArray)(json["d"]["relationships"] ?? new JArray());
                            privateChannelsData = (JArray)(json["d"]["private_channels"] ?? new JArray());
                            // Only uncomment if you need to look at the READY event data as this is a large payload.
                            // Debug.WriteLine(json["d"] != null ? wsJsonEvt : "null");

                            await HandleReadyEvt(wsJsonEvt);
                            break;
                        default:
                            // Only uncomment if you want to debug an event from Discord, this is a mess in the console.
                            // Debug.WriteLine($"[WS] Unhandled event: {eventType}, data: {json["d"]?.ToString(Formatting.None)}");
                            break;
                        case "VOICE_STATE_UPDATE":
                            Debug.WriteLine("Discord sent the state of calling, will handle later!");
                            break;
                        case "VOICE_SERVER_UPDATE":
                            HandleVoiceServerUpdate(json["d"]);
                            break;
                    }
                    break;

                case 10: // Hello from Discord, meaning we're connected.
                    Debug.WriteLine("Discord said hello to us, hello Discord!");
                    heartbeatInterval = json["d"]?["heartbeat_interval"] != null
                                        ? (int)json["d"]["heartbeat_interval"]
                                        : 0;

                    await SendIdentify();
                    Task.Run(() => SendHeartbeat());
                    break;

                case 11: // Heartbeat ack from Discord
                    Debug.WriteLine("Heartbeat was acknowledged by Discord.");
                    break;

                default:
                    Debug.WriteLine($"Unknown op code: {opCode}, data: {data}");
                    break;
            }
        }

        // This is Discord's flush when using zlib-stream
        private bool EndsWithFlushSuffix(byte[] data)
        {
            if (data.Length < 4) return false;
            return data[data.Length - 4] == 0x00 &&
                   data[data.Length - 3] == 0x00 &&
                   data[data.Length - 2] == 0xFF &&
                   data[data.Length - 1] == 0xFF;
        }

        public IEnumerable<JObject> GetUserChannels(bool orderByRecent)
        {
            var channels = privateChannelsData
                .OfType<JObject>()
                .Where(c =>
                {
                    int type = c["type"] != null ? (int)c["type"] : 0;
                    return type == 1 || type == 3;
                });

            if (orderByRecent)
            {
                channels = channels
                    .OrderByDescending(c =>
                        c["last_message_id"] != null ? (string)c["last_message_id"] : "0");
            }

            return channels;
        }
    }
}