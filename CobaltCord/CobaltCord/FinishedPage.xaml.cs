using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using CobaltCord.Classes;

namespace CobaltCord
{
    public sealed partial class FinishedPage : Page
    {
        public FinishedPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsMgr.FinishedWelcome = true;
        }

        private void finishButton_Click(object sender, RoutedEventArgs e)
        {
            // Travels to the main client in the future
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
