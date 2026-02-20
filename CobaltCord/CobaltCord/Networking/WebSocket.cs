using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebSocketStreamer;
using WebSocketStreamer.Networking;
using CobaltCord.Classes;
using Newtonsoft.Json.Linq;

namespace CobaltCord.Networking
{
    public class WebSocket
    {
        // WebSocketStreamer properties
        private WebSocketStreamerClient _client;
        private WebSocketStreamerSend _sender;

        // Discord WS properties
        private string gatewayUrl = "wss://gateway.discord.gg/?v=9&encoding=json";
        private string dscToken;

        // WebSocket properties
        private bool _isConnected = false;
        public bool _canCheckData = false;
        private int heartbeatInterval;

        public WebSocket()
        {
            dscToken = SettingsMgr.DiscordTkn;
        }

        public async Task StartSocket()
        {
            try
            {
                _client = new WebSocketStreamerClient(gatewayUrl);
                _client.MessageReceived += HandleMessage;

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

        private async Task HandleReadyEvt(string data)
        {
            var parsedReady = JObject.Parse(data);
            UserStatusMgr.HandleUserStatus(parsedReady);
        }

        private async void HandleMessage(string data)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<JObject>(data);
                string opCode = json["op"] != null ? (string)json["op"] : "";

                switch (opCode)
                {
                    case "0":
                        string eventType = json["t"] != null ? (string)json["t"] : "";
                        switch (eventType)
                        {
                            case "READY":
                                string wsJsonEvt = json["d"].ToString(Formatting.None);
                                // Only uncomment if you need to look at the READY event data as this is a large payload.
                                // Debug.WriteLine(json["d"] != null ? wsJsonEvt : "null");

                                await HandleReadyEvt(wsJsonEvt);
                                _canCheckData = true;
                                break;
                            default:
                                Debug.WriteLine($"[WS] Unhandled event: {eventType}, data: {json["d"]?.ToString(Formatting.None)}");
                                break;
                        }
                        break;

                    case "10": // Hello from Discord, meaning we're connected.
                        Debug.WriteLine("Discord said hello to us, hello Discord!");
                        heartbeatInterval = json["d"]?["heartbeat_interval"] != null
                                            ? (int)json["d"]["heartbeat_interval"]
                                            : 0;

                        await SendIdentify();
                        Task.Run(() => SendHeartbeat());
                        break;

                    case "11": // Heartbeat ack from Discord
                        Debug.WriteLine("Heartbeat was acknowledged by Discord.");
                        break;

                    default:
                        Debug.WriteLine($"Unknown op code: {opCode}, data: {data}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing message: {ex.Message}");
            }
        }
    }
}