using Windows.Storage;

namespace CobaltCord.Classes
{
    static class MessageCache
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

        private static string GetKey(string userId)
        {
            return "lastmsg_" + userId;
        }

        public static string GetLastMessage(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return GetString(GetKey(userId));
        }

        public static void SetLastMessage(string userId, string message)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            SetString(GetKey(userId), message);
        }

        public static void ClearMessage(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            string key = GetKey(userId);

            if (localSettings.Values.ContainsKey(key))
                localSettings.Values.Remove(key);
        }
    }
}