using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CobaltCord
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void qrButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(QRPage));
        }
    }
}
