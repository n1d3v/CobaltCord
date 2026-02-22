using System;

namespace CobaltCord.Classes
{
    class HelperMethods
    {
        private const int AVATAR_SIZE = 64;

        public static Tuple<string, string> ParseCombinedId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Tuple.Create<string, string>(null, null);

            var parts = id.Split('|');

            if (parts.Length != 2)
                return Tuple.Create<string, string>(null, null);

            return Tuple.Create(parts[0], parts[1]);
        }

        public static string GetAvatarUrl(string Id, string Hash, bool isServer, bool isGC)
        {
            if (isServer)
                return $"https://cdn.discordapp.com/icons/{Id}/{Hash}.png?size={AVATAR_SIZE}";

            if (isGC)
                return $"https://cdn.discordapp.com/channel-icons/{Id}/{Hash}.png?size={AVATAR_SIZE}";

            return $"https://cdn.discordapp.com/avatars/{Id}/{Hash}.png?size={AVATAR_SIZE}";
        }
    }
}