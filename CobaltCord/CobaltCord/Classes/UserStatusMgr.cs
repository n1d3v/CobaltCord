using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CobaltCord.Classes
{
    internal class UserStatusMgr
    {
        public delegate void StatusUpdatedHandler(string userId, string status, string customStatus);
        public static event StatusUpdatedHandler StatusUpdated;

        public class StatusData
        {
            public string Status { get; set; }
            public string CustomStatus { get; set; }
        }

        public static class UserStatusStore
        {
            private static readonly Dictionary<string, StatusData> _statuses = new Dictionary<string, StatusData>();
            private static readonly object _lock = new object();

            public static void UpdateStatus(string userId, string status, string customStatus)
            {
                lock (_lock)
                {
                    if (_statuses.ContainsKey(userId))
                    {
                        _statuses[userId].Status = status;
                        _statuses[userId].CustomStatus = customStatus;
                    }
                    else
                    {
                        _statuses.Add(userId, new StatusData
                        {
                            Status = status,
                            CustomStatus = customStatus
                        });
                    }
                }

                StatusUpdated?.Invoke(userId, status, customStatus);
            }

            public static string GetStatus(string userId)
            {
                lock (_lock)
                {
                    if (_statuses.ContainsKey(userId))
                        return _statuses[userId].Status;

                    return MapStatus("offline");
                }
            }

            public static string GetCustomStatus(string userId)
            {
                lock (_lock)
                {
                    if (_statuses.ContainsKey(userId))
                        return _statuses[userId].CustomStatus;
                    return null;
                }
            }

            public static bool ContainsUser(string userId)
            {
                lock (_lock)
                {
                    return _statuses.ContainsKey(userId);
                }
            }

            public static void Clear()
            {
                lock (_lock)
                {
                    _statuses.Clear();
                }
            }
        }

        private static string MapStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "Unknown";

            switch (status.ToLower())
            {
                case "online":
                    return "Online";

                case "offline":
                    return "Offline";

                case "idle":
                    return "Idle";

                case "dnd":
                    return "Do not disturb";

                case "invisible":
                    return "Invisible";

                default:
                    return char.ToUpper(status[0]) + status.Substring(1);
            }
        }

        public static async Task HandleUserStatus(JObject messageData)
        {
            await Task.Run(() =>
            {
                var userSettings = messageData["user_settings"] as JObject;
                if (userSettings != null)
                {
                    string rawMainStatus = userSettings["status"] != null
                        ? MapStatus(userSettings["status"].ToString())
                        : "Unknown";

                    string rawCustomStatus = string.Empty;
                    var customStatusObj = userSettings["custom_status"] as JObject;
                    if (customStatusObj != null && customStatusObj["text"] != null)
                    {
                        rawCustomStatus = customStatusObj["text"].ToString();
                    }

                    UserStatusStore.UpdateStatus("0", rawMainStatus, rawCustomStatus);
                }

                var presences = messageData["presences"] as JArray;
                if (presences != null)
                {
                    foreach (var presence in presences)
                    {
                        var user = presence["user"] as JObject;
                        if (user == null) continue;

                        string userId = user["id"] != null ? user["id"].ToString() : null;
                        if (userId == null) continue;

                        string status = presence["status"] != null
                            ? MapStatus(presence["status"].ToString())
                            : "Offline";
                        string customStatus = string.Empty;

                        var activities = presence["activities"] as JArray;
                        if (activities != null)
                        {
                            foreach (var activity in activities)
                            {
                                int type = activity["type"] != null ? (int)activity["type"] : -1;

                                if (type == 0 && activity["name"] != null)
                                {
                                    customStatus = "Playing " + activity["name"].ToString();
                                    break;
                                }
                                else if (type == 1 && activity["details"] != null)
                                {
                                    customStatus = "Streaming " + activity["details"].ToString();
                                    break;
                                }
                                else if (type == 2 && activity["name"] != null)
                                {
                                    customStatus = "Listening to " + activity["name"].ToString();
                                    break;
                                }
                                else if (type == 4 && activity["state"] != null)
                                {
                                    customStatus = activity["state"].ToString();
                                    break;
                                }
                            }
                        }

                        UserStatusStore.UpdateStatus(userId, status, customStatus);
                    }
                }
            });
        }
    }
}