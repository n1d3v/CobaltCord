using System;
using System.Diagnostics;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using System.Net.Http;
using CobaltCord.Classes;
using CobaltCord.Networking;
using CobaltCord.UserControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace CobaltCord
{
    public sealed partial class ConversationPage : Page
    {
        private string channelId;

        internal static readonly API api = API.Instance;
        // private WebSocket _webSocket;
        private string dscToken;

        private const int MAX_MESSAGES_LIMIT = 30;
        public ObservableCollection<ListMsgItem> MessagesData { get; set; }
            = new ObservableCollection<ListMsgItem>();

        public ConversationPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        private async Task LoadMessages()
        {
            string msgContent = await api.SendAPI($"channels/{channelId}/messages?limit={MAX_MESSAGES_LIMIT}", HttpMethod.Get, dscToken, null, null, null, null);
            var parsedMsgContent = JArray.Parse(msgContent);

            // Keep track of the current values
            string lastMessageText = null;
            string lastMessageTime = null;

            for (int i = parsedMsgContent.Count - 1; i >= 0; i--)
            {
                var message = parsedMsgContent[i];

                string messageText = message["content"].Value<string>();
                string messageTimestamp = message["timestamp"].Value<string>();
                string messageId = message["id"].Value<string>();
                string messageAuthorHash = message["author"]["avatar"].Value<string>();
                string messageAuthor = message["author"]["global_name"].Value<string>();
                string messageAuthorId = message["author"]["id"].Value<string>();
                string formattedTime = DateTimeOffset.Parse(messageTimestamp).ToLocalTime().ToString("hh:mm tt");

                MessagesData.Add(new ListMsgItem
                {
                    AuthorName = messageAuthor,
                    AuthorId = messageAuthorId,
                    AuthorHash = messageAuthorHash,
                    MessageId = messageId,
                    MessageText = messageText,
                    MessageTime = formattedTime
                });

                lastMessageText = $"{messageAuthor}: {messageText}";
                lastMessageTime = formattedTime;
            }

            if (!string.IsNullOrEmpty(lastMessageText))
            {
                MessageCache.ClearAll(channelId);
                MessageCache.SetLastMessage(channelId, lastMessageText);
                MessageCache.SetLastTime(channelId, lastMessageTime);
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var data = e.Parameter as HelperClasses.ConversationNavData;
            if (data != null)
            {
                var channelIdParser = HelperMethods.ParseCombinedId(data.CombinedId);

                // Set data for the page, channel ID is really important!
                channelId = channelIdParser.Item2;
                chatTextName.Text = data.DisplayName;
                dscToken = SettingsMgr.DiscordTkn;

                try
                {
                    // Now we load messages!
                    await LoadMessages();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An exception occurred! {ex.Message}");
                }
            }
        }

        private void backButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ClientPage));
        }

        private async void sendButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var chatPayload = new { content = messageBox.Text, flags = 0, mobile_network_type = "unknown", tts = false };
            await api.SendAPI($"channels/{channelId}/messages", HttpMethod.Post, dscToken, chatPayload, null, null, null);

            messageBox.Text = string.Empty;
        }

        private void callPersonButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CallPage), new HelperClasses.CallNavData { ChannelId = channelId });
        }

        // Fixes the bug with a transparent background where the text does not appear
        private void messageBox_LostFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            messageBox.Foreground = new SolidColorBrush(Colors.White);
        }

        private void messageBox_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            messageBox.Foreground = new SolidColorBrush(Colors.Black);
        }
    }
}