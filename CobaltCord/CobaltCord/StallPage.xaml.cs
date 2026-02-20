using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

namespace CobaltCord
{
    public sealed partial class StallPage : Page
    {
        public StallPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // We delay the switching to the next page to have a nice animation.
            // Sorry if this causes an inconvenience!
            await Task.Delay(2000);
            Frame.Navigate(typeof(WelcomePage));
        }
    }
}
