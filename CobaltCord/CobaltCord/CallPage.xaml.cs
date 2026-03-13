using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using CobaltCord.Networking;
using CobaltCord.Classes;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;

namespace CobaltCord
{
    public sealed partial class CallPage : Page
    {
        private string channelId;
        private string dscToken;
        internal static readonly API api = API.Instance;
        // private WebSocket _webSocket;

        public CallPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var data = e.Parameter as HelperClasses.CallNavData;
            if (data != null)
            {
                // Set the channel ID for the call UI
                channelId = data.ChannelId;
                dscToken = SettingsMgr.DiscordTkn;

                try
                {
                    // await RingConversation();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An exception occurred! {ex.Message}");
                }
            }
        }
    }
}
