using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CobaltCord.Networking;
using CobaltCord.Classes;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CobaltCord
{
    public sealed partial class LoginPage : Page
    {
        public IDictionary<string, object> DefaultViewModel { get; } = new Dictionary<string, object>();

        internal static readonly API api = API.Instance;
        private string dscToken = null;

        public LoginPage()
        {
            this.InitializeComponent();
            DefaultViewModel["Item"] = new object();
        }

        private void passwordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                nextButton.IsEnabled = false;
            }
            else
            {
                dscToken = passwordBox.Password;
                nextButton.IsEnabled = true;
            }
        }

        private void qrButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(QRPage));
        }


        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(WelcomePage));
        }

        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            bool CanNavigate = false;
            string tokenChk = await api.SendAPI("users/@me/billing/country-code", HttpMethod.Get, dscToken, null, null, null, null);

            if (!string.IsNullOrWhiteSpace(tokenChk))
            {
                try
                {
                    var json = JObject.Parse(tokenChk);
                    if (!string.IsNullOrEmpty(json["country_code"]?.Value<string>()))
                    {
                        CanNavigate = true;
                    }
                }
                catch
                {
                    CanNavigate = false;
                }
            }
            else
            {
                CanNavigate = false;
            }

            if (CanNavigate)
            {
                SettingsMgr.DiscordTkn = dscToken;
                Frame.Navigate(typeof(FinishedPage));
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "woah... something went wrong!",
                    Content = new TextBlock
                    {
                        Text = "Your token has failed the check that makes sure it is valid. Please make sure your token is completely valid!",
                        TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap
                    },
                    PrimaryButtonText = "ok",
                    SecondaryButtonText = "but it's right?"
                };

                dialog.ShowAsync().Completed = (info, status) =>
                {
                    if (info.GetResults() == ContentDialogResult.Secondary)
                    {
                        var correctDialog = new ContentDialog
                        {
                            Title = "are you absolutely sure?",
                            Content = new TextBlock
                            {
                                Text = "You can force your current token into CobaltCord's settings, however this may cause problems. If you are sure that you entered it correctly, you can go ahead.",
                                TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap
                            },
                            PrimaryButtonText = "i'm sure",
                            SecondaryButtonText = "nevermind"
                        };

                        correctDialog.ShowAsync().Completed = (information, statusOfStatus) =>
                        {
                            if (information.GetResults() == ContentDialogResult.Primary)
                            {
                                SettingsMgr.DiscordTkn = dscToken;
                                Frame.Navigate(typeof(FinishedPage));
                            }
                            else
                            {
                                // Do nothing, the app will just go back to the login page.
                            }
                        };
                    }
                };
            }
        }
    }
}
