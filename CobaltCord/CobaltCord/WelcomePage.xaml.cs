using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;

namespace CobaltCord
{
    public sealed partial class WelcomePage : Page
    {
        public IDictionary<string, object> DefaultViewModel { get; } = new Dictionary<string, object>();

        public WelcomePage()
        {
            this.InitializeComponent();
            DefaultViewModel["Item"] = new object();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }
    }
}