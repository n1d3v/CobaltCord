using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using CobaltCord.Networking;
using CobaltCord.Classes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CobaltCord
{
    public sealed partial class ClientPage : Page
    {
        internal static readonly API api = new API();
        private WebSocket _webSocket;
        private string dscToken;

        // Magic numbers used throughout the code for defining Discord stuff
        private const int AVATAR_SIZE = 64;

        public ClientPage()
        {
            this.InitializeComponent();
            dscToken = SettingsMgr.DiscordTkn;
            _webSocket = new WebSocket();

            // We'll use this as a way to use async.
            this.Loaded += ClientPage_Loaded;
        }

        private async Task InitializeUserInfo()
        {
            string userInfo = await api.SendAPI("users/@me", HttpMethod.Get, dscToken, null, null, null, null);
            var parsedInfo = JObject.Parse(userInfo);

            string displayName = string.Empty;
            string avatarHash = string.Empty;

            JToken globalNameToken;
            if (parsedInfo.TryGetValue("global_name", out globalNameToken) && globalNameToken.Type != JTokenType.Null) { displayName = globalNameToken.ToString(); }

            JToken avatarHashToken;
            if (parsedInfo.TryGetValue("avatar", out avatarHashToken) && avatarHashToken.Type != JTokenType.Null) { avatarHash = avatarHashToken.ToString(); }

            if (string.IsNullOrWhiteSpace(SettingsMgr.DiscordUID))
            {
                JToken UIDToken;
                if (parsedInfo.TryGetValue("id", out UIDToken) && UIDToken.Type != JTokenType.Null) { SettingsMgr.DiscordUID = UIDToken.ToString(); }
            }
            else
            {
                // Do nothing and ignore the code above, we don't need it.
            }

            // Set the user data that we have collected.
            string avatarUrl = GetAvatarUrl(SettingsMgr.DiscordUID, avatarHash, false, false);
            await SetImageFromUrl(UserAvatar, avatarUrl);

            // Gets the main users custom status to use at the bottom of the PseudoCommandBar.
            while (!_webSocket._canCheckData)
            {
                // Looking to replace this in the future with an event handler.
                await Task.Delay(100);
            }

            string custStatus = UserStatusMgr.UserStatusStore.GetCustomStatus("0") ?? "Add a custom status...";

            usernameText.Text = displayName;
            statusText.Text = custStatus;
        }

        private async void ClientPage_Loaded(object sender, RoutedEventArgs e)
        {
            await _webSocket.StartSocket();
            await InitializeUserInfo();
        }

        private async Task SetImageFromUrl(Image targetImage, string url)
        {
            if (string.IsNullOrWhiteSpace(url) || targetImage == null)
                return;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var bytes = await client.GetByteArrayAsync(url);
                    using (var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream())
                    {
                        await stream.WriteAsync(bytes.AsBuffer());
                        stream.Seek(0);

                        BitmapImage bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(stream);

                        targetImage.Source = bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load image: {ex.Message}");
            }
        }

        public string GetAvatarUrl(string Id, string Hash, bool isServer, bool isGC)
        {
            if (isServer)
                return $"https://cdn.discordapp.com/icons/{Id}/{Hash}.png?size={AVATAR_SIZE}";

            if (isGC)
                return $"https://cdn.discordapp.com/channel-icons/{Id}/{Hash}.png?size={AVATAR_SIZE}";

            return $"https://cdn.discordapp.com/avatars/{Id}/{Hash}.png?size={AVATAR_SIZE}";
        }

        /* 
            protected override void OnNavigatedTo(NavigationEventArgs e)
            {

            } 
        */
    }
}
