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
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Linq;

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

            for (int i = parsedMsgContent.Count - 1; i >= 0; i--)
                AddMessage(parsedMsgContent[i]);

            if (MessagesData.Count > 0)
            {
                var last = MessagesData.Last();
                MessageCache.ClearAll(channelId);
                MessageCache.SetLastMessage(channelId, $"{last.AuthorName}: {last.MessageText}");
                MessageCache.SetLastTime(channelId, last.MessageTime);
            }

            ScrollToBottom();
        }

        private void AddMessage(JToken messageData)
        {
            string messageText = messageData["content"].Value<string>();
            string messageTimestamp = messageData["timestamp"].Value<string>();
            string messageId = messageData["id"].Value<string>();
            string messageAuthorHash = messageData["author"]["avatar"]?.Value<string>();
            string messageAuthor = messageData["author"]["global_name"]?.Value<string>()
                                   ?? messageData["author"]["username"].Value<string>();
            string messageAuthorId = messageData["author"]["id"].Value<string>();
            string formattedTime = DateTimeOffset.Parse(messageTimestamp).ToLocalTime().ToString("hh:mm tt");

            bool isContinuation = MessagesData.Count > 0 &&
                                  MessagesData.Last().AuthorId == messageAuthorId;

            if (!isContinuation && MessagesData.Count > 0 && MessagesData.Last().IsContinuation)
                MessagesData.Last().IsLastContinuation = true;

            MessagesData.Add(new ListMsgItem
            {
                AuthorName = messageAuthor,
                AuthorId = messageAuthorId,
                AuthorHash = messageAuthorHash,
                MessageId = messageId,
                MessageText = messageText,
                MessageTime = formattedTime,
                IsContinuation = isContinuation
            });
        }

        private void ScrollToBottom()
        {
            if (MessagesData.Count > 0)
                MessagesList.ScrollIntoView(MessagesData.Last());
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
            // Fixes a bug where it doesn't actually send a message, because it hasn't finished typing.
            this.Focus(Windows.UI.Xaml.FocusState.Programmatic);

            string msgText = messageBox.Text;
            if (string.IsNullOrWhiteSpace(msgText)) return;

            messageBox.Text = string.Empty;

            var chatPayload = new { content = msgText, flags = 0, mobile_network_type = "unknown", tts = false };
            string chatResponse = await api.SendAPI($"channels/{channelId}/messages", HttpMethod.Post, dscToken, chatPayload, null, null, null);

            var parsedData = JObject.Parse(chatResponse);
            AddMessage(parsedData);

            MessagesList.ScrollIntoView(MessagesData.Last());
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