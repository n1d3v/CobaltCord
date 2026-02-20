using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CobaltCord.Classes;

namespace CobaltCord
{
    public sealed partial class FinishedPage : Page
    {
        public FinishedPage()
        {
            this.InitializeComponent();
        }

        private void finishButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsMgr.FinishedWelcome = true;
            Frame.Navigate(typeof(ClientPage));
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}   