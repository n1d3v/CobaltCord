using Windows.Storage;

namespace CobaltCord.Classes
{
    static class SettingsMgr
    {
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        private static string GetString(string key)
        {
            object value;
            if (localSettings.Values.TryGetValue(key, out value) && value is string)
                return (string)value;
            return null;
        }

        private static void SetString(string key, string value)
        {
            localSettings.Values[key] = value;
        }

        private static bool GetBool(string key)
        {
            object value;
            if (localSettings.Values.TryGetValue(key, out value) && value is bool)
                return (bool)value;
            return false;
        }

        private static void SetBool(string key, bool value)
        {
            localSettings.Values[key] = value;
        }

        public static string DiscordTkn
        {
            get { return GetString("discordTkn"); }
            set { SetString("discordTkn", value); }
        }

        public static string DiscordUID
        {
            get { return GetString("discordUID"); }
            set { SetString("discordUID", value); }
        }

        public static bool FinishedWelcome
        {
            get { return GetBool("finishedWel"); }
            set { SetBool("finishedWel", value); }
        }
    }
}