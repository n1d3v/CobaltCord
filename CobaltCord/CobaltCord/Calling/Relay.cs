using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CobaltCord.Calling
{
    class RelayREST
    {
        public string relayIp = "http://127.0.0.1";
        public int relayPort = 5000;

        public async Task AuthenticateCall(string dscToken, string channelId, string clientVer = null, string clientCN = null)
        {
            // Authenticate with the relay server to do the heavy-lifting for us
            var payload = new
            {
                Token = dscToken,
                ChannelID = channelId,
                ClientVersion = clientVer,
                ClientCodename = clientCN
            };
            string authResponse = await RelayHelper.PostReq($"{relayIp}:{relayPort}/rest/authenticate", payload);

            // Let's now actually connect to the call on our relay
            await RingConversation();
        }

        public async Task RingConversation()
        {
            string ringResponse = await RelayHelper.PostReq($"{relayIp}:{relayPort}/rest/ring", null);
        }
    }

    class RelayServer
    {
    }

    class RelayUDP
    {
    }

    class RelayHelper
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task<string> PostReq(string url, object data)
        {
            string json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
