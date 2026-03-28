using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using CobaltCord.Classes;

namespace CobaltCord.Networking
{
    internal sealed class API
    {
        // This allows us to use one single API instance throughout the entire client
        // Both reducing the RAM usage, and also reducing the risk of getting banned off of Discord if they catch on
        // This is also present in the WebSocket system, which does the same thing.
        private static readonly Lazy<API> _instance = new Lazy<API>(() => new API());
        public static API Instance => _instance.Value;

        // Re-used client (single instance)
        private static readonly HttpClient client = new HttpClient();
        private static readonly ConfigMgr dscMgr = new ConfigMgr();

        private static string XSuperProperties;
        private static readonly string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0";

        private API() { XSuperProperties = dscMgr.GetXSPJson(); }

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
                var content = new MultipartFormDataContent { { new ByteArrayContent(fileData) { Headers = { { "Content-Type", "application/octet-stream" } } }, "file", fileName } };

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

            // Discord prefers GZip for sending requests back to us, this is good for loading performance
            // It is unlikely that Discord will use something else if GZip is specified, I haven't seen this anywhere though
            request.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br, zstd");
            request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
            request.Headers.TryAddWithoutValidation("X-Super-Properties", XSuperProperties);

            try
            {
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await ReadResponse(response);
                }

                string errorResponse = await ReadResponse(response);
                Debug.WriteLine($"[DEBUG] Request failed: {response.StatusCode} - {errorResponse}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DEBUG] Error sending request: {ex.Message}");
                Debug.WriteLine($"[DEBUG] URL used: {url}");
            }

            return string.Empty;
        }

        private static async Task<string> ReadResponse(HttpResponseMessage response)
        {
            // Raw bytes from the request response
            byte[] rawBytes = await response.Content.ReadAsByteArrayAsync();

            IEnumerable<string> encodings;
            bool hasContentEncoding = response.Content.Headers.TryGetValues("Content-Encoding", out encodings);

            if (hasContentEncoding)
            {
                foreach (string encoding in encodings)
                {
                    // If it's GZip, then just decompress the GZip
                    if (encoding.Equals("gzip", StringComparison.OrdinalIgnoreCase)) { return await DecompressGZip(rawBytes); }
                }
            }
            return Encoding.UTF8.GetString(rawBytes, 0, rawBytes.Length);
        }

        private static async Task<string> DecompressGZip(byte[] compressedData)
        {
            using (var compressedStream = new MemoryStream(compressedData))
            using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                await gzipStream.CopyToAsync(resultStream);
                return Encoding.UTF8.GetString(resultStream.ToArray(), 0, (int)resultStream.Length);
            }
        }
    }
}