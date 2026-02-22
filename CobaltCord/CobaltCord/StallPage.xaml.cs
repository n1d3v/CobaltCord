using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Collections.Generic;
using CobaltCord.Classes;
using System.Diagnostics;

namespace CobaltCord
{
    public sealed partial class StallPage : Page
    {
        public IDictionary<string, object> DefaultViewModel { get; } = new Dictionary<string, object>();

        private WelcomePage _welcomePage;
        private LoginPage _loginPage;
        private FinishedPage _finishedPage;
        private ClientPage _clientPage;

        public StallPage()
        {
            this.InitializeComponent();
            DefaultViewModel["Item"] = new object();

            // Preload the pages beforehand so we can have smooth transitions.
            _welcomePage = new WelcomePage();
            _loginPage = new LoginPage();
            _finishedPage = new FinishedPage();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // We delay the switching to the next page to have a nice animation.
            // Sorry if this causes an inconvenience!
            if (SettingsMgr.FinishedWelcome)
            {
                await Task.Delay(1500);
                _clientPage = new ClientPage();
                Frame.Navigate(typeof(ClientPage));
            }
            else
            {
                await Task.Delay(1500);
                Frame.Navigate(typeof(WelcomePage));
            }
        }
    }
}