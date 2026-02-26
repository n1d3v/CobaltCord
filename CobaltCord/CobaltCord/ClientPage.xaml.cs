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
using CobaltCord.UserControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace CobaltCord
{
    public sealed partial class ClientPage : Page
    {
        internal static readonly API api = API.Instance;
        private WebSocket _webSocket;
        private string dscToken;

        // Magic numbers used throughout the code for defining Discord stuff
        private const int DM_CHANNEL_TYPE = 1;
        private const int GROUP_CHANNEL_TYPE = 3;

        // The list for direct messages on the Pivot
        public ObservableCollection<ListItem> DirectMessages { get; set; }
            = new ObservableCollection<ListItem>();

        public ClientPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Required;
            dscToken = SettingsMgr.DiscordTkn;
            _webSocket = WebSocket.Instance;

            // We'll use this as a way to use async.
            this.Loaded += ClientPage_Loaded;
        }

        private async Task InitializeUserInfo()
        {
            try
            {
                if (!string.IsNullOrEmpty(SettingsMgr.DiscordDPN) || !string.IsNullOrEmpty(SettingsMgr.DiscordSTS))
                {
                    usernameText.Text = SettingsMgr.DiscordDPN;
                    statusText.Text = SettingsMgr.DiscordSTS;
                }

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
                string avatarUrl = HelperMethods.GetAvatarUrl(SettingsMgr.DiscordUID, avatarHash, false, false);
                await AvatarHelper.SetAvatarFromHash(UserAvatar, SettingsMgr.DiscordUID, avatarHash, avatarUrl);

                // Gets the main users custom status to use at the bottom of the PseudoCommandBar.
                string custStatus = UserStatusMgr.UserStatusStore.GetCustomStatus("0") ?? "Add a custom status...";

                SettingsMgr.DiscordDPN = displayName;
                SettingsMgr.DiscordSTS = custStatus;

                usernameText.Text = displayName;
                statusText.Text = custStatus;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An exception has occured: {ex.Message}");
            }
        }

        private async Task InitializePivotLists()
        {
            if (SettingsMgr.CachingWarning)
            {
                // Continue along with the application as normal.
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "letting you know...",
                    Content = new TextBlock
                    {
                        Text = "Your messages list appears empty because you haven’t opened any conversations yet.",
                        TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap
                    },
                    PrimaryButtonText = "explain further",
                    SecondaryButtonText = "alright, I get it"
                };

                dialog.ShowAsync().Completed = (info, status) =>
                {
                    if (info.GetResults() == ContentDialogResult.Primary)
                    {
                        var cacheDialog = new ContentDialog
                        {
                            Title = "the app caches conversations",
                            Content = new TextBlock
                            {
                                Text = "CobaltCord caches your messages. Cached messages will appear in your messages list, and new messages that you receive while you're in the app will show up automatically.",
                                TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap
                            },
                            PrimaryButtonText = "understood!"
                        };

                        cacheDialog.ShowAsync().Completed = (information, statusOfStatus) => { if (information.GetResults() == ContentDialogResult.Primary) { SettingsMgr.CachingWarning = true; } };
                    }
                    else if (info.GetResults() == ContentDialogResult.Secondary) { SettingsMgr.CachingWarning = true; }
                };
            }

            int currentDMCount = 0;
            var dscChannels = _webSocket.GetUserChannels(true);

            int totalDMs = dscChannels.Count(c => (c["type"]?.Value<int>() ?? 0) == DM_CHANNEL_TYPE);
            var dmItemsToAdd = new List<ListItem>();

            foreach (var channel in dscChannels)
            {
                int type = channel["type"]?.Value<int>() ?? 0;

                if (type == DM_CHANNEL_TYPE)
                {
                    var recipients = channel["recipients"] as JArray;
                    if (recipients == null || recipients.Count == 0) continue;

                    var recipient = recipients[0] as JObject;
                    if (recipient == null) continue;

                    string userId = recipient["id"]?.Value<string>();
                    string channelId = channel["id"]?.Value<string>();
                    string combinedId = $"{userId}|{channelId}";

                    string displayName = recipient["global_name"]?.Value<string>();
                    string dscUserName = recipient["username"]?.Value<string>();
                    string dscAvatarHash = recipient["avatar"]?.Value<string>();

                    currentDMCount++;
                    ShowProgressIndicator(true, $"Downloading profile pictures ({currentDMCount}/{totalDMs})");

                    string lastMsg = MessageCache.GetLastMessage(channelId);

                    string avatarUrl = HelperMethods.GetAvatarUrl(userId, dscAvatarHash, false, false);
                    await AvatarHelper.SetAvatarFromHash(DoNotUnhideThisImage, userId, dscAvatarHash, avatarUrl);

                    dmItemsToAdd.Add(new ListItem
                    {
                        Name = displayName,
                        SecondaryText = lastMsg,
                        CombinedId = combinedId
                    });
                }
                else if (type == GROUP_CHANNEL_TYPE)
                {
                    var recipients = channel["recipients"] as JArray;
                    int recipientCount = recipients?.Count ?? 0;
                    int memberCount = recipientCount + 1;

                    HelperClasses.User[] members = null;

                    if (recipients != null && recipients.Count > 0)
                    {
                        members = recipients
                            .OfType<JObject>()
                            .Select(r =>
                            {
                                var globalNameToken = r["global_name"];
                                var usernameToken = r["username"];
                                var idToken = r["id"];

                                var globalName = globalNameToken != null ? globalNameToken.Value<string>() : null;
                                var username = usernameToken != null ? usernameToken.Value<string>() : null;
                                var id = idToken != null ? idToken.Value<string>() : null;

                                return new HelperClasses.User(
                                    globalName ?? username ?? "Unknown",
                                    username ?? "Unknown",
                                    id ?? "0"
                                );
                            })
                            .ToArray();
                    }

                    string channelId = channel["id"]?.Value<string>();
                    string groupName = channel["name"]?.Value<string>();

                    string combinedId = $"Group|{channelId}";

                    dmItemsToAdd.Add(new ListItem
                    {
                        Name = groupName,
                        SecondaryText = $"{memberCount} members in total",
                        CombinedId = combinedId
                    });
                }
            }

            foreach (var item in dmItemsToAdd)
            {
                DirectMessages.Add(item);
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedItem = e.ClickedItem as ListItem;
            if (clickedItem != null)
            {
                Debug.WriteLine("Clicked the item, now loading conversation page.");
                Frame.Navigate(typeof(ConversationPage), new HelperClasses.ConversationNavData
                {
                    CombinedId = clickedItem.CombinedId,
                    DisplayName = clickedItem.Name
                });
            }
        }

        private async void ClientPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowProgressIndicator(true, "Initializing WebSockets...");
                await _webSocket.StartSocket();

                ShowProgressIndicator(true, "Waiting for data to come through...\nThis may take a bit...");
                await WaitForReadyEvt();

                ShowProgressIndicator(true, "Loading your user data...");
                await InitializeUserInfo();

                ShowProgressIndicator(true, "Loading messages...");
                await InitializePivotLists();

                ShowProgressIndicator(false, string.Empty);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception happened when loading client data: {ex}");
            }
        }

        private async Task WaitForReadyEvt()
        {
            var tcs = new TaskCompletionSource<bool>();

            EventHandler handler = null;
            handler = (s, e) =>
            {
                _webSocket.ReadyReceived -= handler;
                tcs.TrySetResult(true);
            };

            _webSocket.ReadyReceived += handler;

            await tcs.Task;
        }

        private void ShowProgressIndicator(bool isVisible, string text = "")
        {
            ProgressRingControl.IsActive = isVisible;
            ProgressRingControl.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

            ProgressText.Text = text;
            ProgressText.Visibility = string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        /* 
            protected override void OnNavigatedTo(NavigationEventArgs e)
            {

            } 
        */
    }
}