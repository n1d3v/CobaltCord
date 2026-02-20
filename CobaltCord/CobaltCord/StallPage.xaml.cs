using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Collections.Generic;
using CobaltCord.Classes;

namespace CobaltCord
{
    public sealed partial class StallPage : Page
    {
        public IDictionary<string, object> DefaultViewModel { get; } = new Dictionary<string, object>();

        public StallPage()
        {
            this.InitializeComponent();
            DefaultViewModel["Item"] = new object();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // We delay the switching to the next page to have a nice animation.
            // Sorry if this causes an inconvenience!
            await Task.Delay(1500);
            if (SettingsMgr.FinishedWelcome)
            {
                // Travels to the main client in the future
            }
            else
            {
                Frame.Navigate(typeof(WelcomePage));
            }
        }
    }
}
