using System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using WebSocketStreamer;
using WebSocketStreamer.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Web.Http;
using CobaltCord.Networking;
using CobaltCord.Classes;
using System.Collections.Generic;
using System.Text;

namespace CobaltCord
{
    public sealed partial class QRPage : Page
    {
        private AuthSocket authSocket;
        public IDictionary<string, object> DefaultViewModel { get; } = new Dictionary<string, object>();

        public QRPage()
        {
            this.InitializeComponent();
            DefaultViewModel["Item"] = new object();
            this.Loaded += QRPage_Loaded;
            authSocket = new AuthSocket(this, this.Dispatcher);
        }

        private async void QRPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await authSocket.StartSocket();
        }

        private void backButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

        public async Task ShowQRCode(string content)
        {
            try
            {
                BitmapImage qrBitmap = await authSocket.GenerateQRCode(content);

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    dscQR.Source = qrBitmap;
                    await UpdateQRText(string.Empty);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to generate QR code: {ex.Message}");
            }
        }

        public async Task UpdateQRText(string text)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => dscQRUrl.Text = text);
        }

        public async Task ContinueToFinished()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { Frame.Navigate(typeof(FinishedPage)); });
        }
    }

    public class AuthSocket
    {
        private WebSocketStreamerClient _client;
        private WebSocketStreamerSend _sender;
        private WebSocketStreamerErrorHandler _errorHandler;

        private string gatewayUrl = "wss://remote-auth-gateway.discord.gg/?v=2";
        private string authUrl = "https://discord.com/ra/";
        private string identifyPayload;

        private CryptographicKey _cryptoKey;
        private CoreDispatcher _dispatcher;
        private Page _context;

        internal static readonly API api = new API();

        public AuthSocket(Page context, CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _context = context;
        }

        private string GenerateEncodedKey()
        {
            var provider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaOaepSha256);
            _cryptoKey = provider.CreateKeyPair(2048);

            IBuffer pubKeyBuffer = _cryptoKey.ExportPublicKey(CryptographicPublicKeyBlobType.X509SubjectPublicKeyInfo);

            byte[] pubKeyBytes;
            CryptographicBuffer.CopyToByteArray(pubKeyBuffer, out pubKeyBytes);

            return Convert.ToBase64String(pubKeyBytes);
        }

        private string DecryptNonce(string encNonce)
        {
            byte[] encBytes = Convert.FromBase64String(encNonce);
            IBuffer encBuffer = CryptographicBuffer.CreateFromByteArray(encBytes);
            IBuffer plainBuffer = CryptographicEngine.Decrypt(_cryptoKey, encBuffer, null);

            byte[] plainBytes;
            CryptographicBuffer.CopyToByteArray(plainBuffer, out plainBytes);

            return Convert.ToBase64String(plainBytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private string DecryptRSA(string base64Input)
        {
            byte[] encBytes = Convert.FromBase64String(base64Input);
            IBuffer encBuffer = CryptographicBuffer.CreateFromByteArray(encBytes);
            IBuffer plainBuffer = CryptographicEngine.Decrypt(_cryptoKey, encBuffer, null);

            byte[] plainBytes;
            CryptographicBuffer.CopyToByteArray(plainBuffer, out plainBytes);

            return Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);
        }

        public async Task StartSocket()
        {
            try
            {
                _client = new WebSocketStreamerClient(gatewayUrl);

                _client.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0");
                _client.AddHeader("Origin", "https://discord.com");

                _client.MessageReceived += HandleMessage;

                await _client.Connect();

                _errorHandler = new WebSocketStreamerErrorHandler(_client);
                _sender = new WebSocketStreamerSend(_client.Socket);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing WebSocket: {ex.GetType()} - {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private async void SendAuthPayload()
        {
            identifyPayload = JsonConvert.SerializeObject(new
            {
                op = "init",
                encoded_public_key = GenerateEncodedKey()
            });
            await _sender.SendText(identifyPayload);
        }

        private async void HandleNonceProof(string data)
        {
            var json = JObject.Parse(data);
            // Find the encrypted_nonce Discord sends to us
            string encryptedNonce = json["encrypted_nonce"]?.Value<string>();
            // Decrypt the nonce using the private_key we generated earlier
            string nonce = DecryptNonce(encryptedNonce);
            // Send proof of the nonce that we decrypted to Discord
            var payload = JsonConvert.SerializeObject(new
            {
                op = "nonce_proof",
                nonce = nonce
            });

            await _sender.SendText(payload);
        }

        private async void HandleQRCode(string data)
        {
            var json = JObject.Parse(data);
            string fingerprintQR = json["fingerprint"]?.Value<string>();
            string fullRA = authUrl + fingerprintQR;

            var page = _context as QRPage;
            if (page != null) { await page.ShowQRCode(fullRA); }
        }

        public async Task<BitmapImage> GenerateQRCode(string content)
        {
            string qrUrl = $"https://qrcodecat.com/api/qrcode?data={content}";

            var httpClient = new HttpClient();
            var buffer = await httpClient.GetBufferAsync(new Uri(qrUrl));
            BitmapImage bitmap = null;

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                bitmap = new BitmapImage();
                using (var memStream = new InMemoryRandomAccessStream())
                {
                    await memStream.WriteAsync(buffer);
                    memStream.Seek(0);
                    await bitmap.SetSourceAsync(memStream);
                }
            });

            return bitmap;
        }

        private async void HandleQRUpdate()
        {
            var page = _context as QRPage;
            if (page != null) { await page.UpdateQRText("Please confirm the login data on your phone."); }
        }

        private async void HandleQRLogin(string data)
        {
            var json = JObject.Parse(data);
            string discordTkt = json["ticket"]?.Value<string>();
            var ticketPayload = new { ticket = discordTkt };

            string encToken = await api.SendAPI("users/@me/remote-auth/login", System.Net.Http.HttpMethod.Post, null, ticketPayload, null, null);

            var encJson = JObject.Parse(encToken);
            string discordEncTkn = encJson["encrypted_token"]?.Value<string>();
            string decTkn = DecryptRSA(discordEncTkn);

            SettingsMgr.DiscordTkn = decTkn;

            var page = _context as QRPage;
            if (page != null) { await page.ContinueToFinished(); }
        }


        private void HandleMessage(string data)
        {
            try
            {
                var json = JObject.Parse(data);
                string op = json["op"]?.Value<string>() ?? "";

                switch (op)
                {
                    case "hello": SendAuthPayload(); break;
                    case "nonce_proof": HandleNonceProof(data); break;
                    case "pending_remote_init": HandleQRCode(data); break;
                    case "pending_ticket": HandleQRUpdate(); break;
                    case "pending_login": HandleQRLogin(data); break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error processing message: {ex.Message}");
            }
        }
    }
}