using Windows.Storage;

namespace CobaltCord.Classes
{
    static class MessageCache
    {
        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;

        // Methods for helping out with the functions below
        private static string GetString(string key)
        {
            object value;
            if (LocalSettings.Values.TryGetValue(key, out value) && value is string)
                return (string)value;

            return null;
        }

        private static void SetString(string key, string value) => LocalSettings.Values[key] = value;

        private static string MsgKey(string channelId) => $"lastmsg_{channelId}";
        private static string TimeKey(string channelId) => $"lasttime_{channelId}";

        // Methods for message functionality
        public static string GetLastMessage(string channelId) => string.IsNullOrWhiteSpace(channelId) ? null : GetString(MsgKey(channelId));

        public static void SetLastMessage(string channelId, string message)
        {
            if (string.IsNullOrWhiteSpace(channelId)) return;
            SetString(MsgKey(channelId), message);
        }

        public static void ClearMessage(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId)) return;
            LocalSettings.Values.Remove(MsgKey(channelId));
        }

        // Methods for time functionality
        public static string GetLastTime(string channelId) => string.IsNullOrWhiteSpace(channelId) ? null : GetString(TimeKey(channelId));

        public static void SetLastTime(string channelId, string time)
        {
            if (string.IsNullOrWhiteSpace(channelId)) return;
            SetString(TimeKey(channelId), time);
        }

        public static void ClearTime(string channelId)
        {
            if (string.IsNullOrWhiteSpace(channelId)) return;
            LocalSettings.Values.Remove(TimeKey(channelId));
        }

        // Clear all of the previous details
        public static void ClearAll(string channelId)
        {
            ClearMessage(channelId);
            ClearTime(channelId);
        }
    }
}