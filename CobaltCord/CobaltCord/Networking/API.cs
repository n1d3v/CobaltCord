using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using CobaltCord.Classes;

namespace CobaltCord.Networking
{
    internal class API
    {
        // Re-used client (Less memory usage)
        private static readonly HttpClient client = new HttpClient();
        private static readonly ConfigMgr dscMgr = new ConfigMgr();

        // Configuration (Firefox 115 ESR on Windows 10)
        private static readonly string XSuperProperties = null;
        private static readonly string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0";

        static API()
        {
            XSuperProperties = dscMgr.GetXSPJson();
        }

        public async Task<string> SendAPI(string endpoint, HttpMethod httpMethod, string token = null, object data = null, byte[] fileData = null, string fileName = null, Dictionary<string, string> headers = null)
        {
            string url = $"https://discord.com/api/v9/{endpoint}";
            var request = new HttpRequestMessage(httpMethod, url);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token);
            }

            if (headers != null)
            {
                foreach (var kvp in headers)
                {
                    request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }

            if (fileData != null && !string.IsNullOrEmpty(fileName))
            {
                var content = new MultipartFormDataContent
                {
                    { new ByteArrayContent(fileData) { Headers = { { "Content-Type", "application/octet-stream" } } }, "file", fileName }
                };

                if (data != null)
                {
                    string jsonData = JsonConvert.SerializeObject(data);
                    content.Add(new StringContent(jsonData, Encoding.UTF8, "application/json"), "payload_json");
                }

                request.Content = content;
            }
            else if ((httpMethod == HttpMethod.Post || httpMethod == HttpMethod.Put) && data != null)
            {
                string jsonData = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            }

            request.Headers.Add("User-Agent", UserAgent);
            request.Headers.Add("X-Super-Properties", XSuperProperties);

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[DEBUG] Request failed: {response.StatusCode} - {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] An error occurred while sending the request: {ex.Message}");
                Debug.WriteLine($"[DEBUG] URL used: {url}");
            }

            return string.Empty;
        }
    }
}