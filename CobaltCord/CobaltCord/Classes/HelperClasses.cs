namespace CobaltCord.Classes
{
    class HelperClasses
    {
        public class User
        {
            public string GlobalName { get; }
            public string Username { get; }
            public string Id { get; }

            public User(string globalName, string username, string id)
            {
                GlobalName = globalName;
                Username = username;
                Id = id;
            }
        }

        public class ConversationNavData
        {
            public string CombinedId { get; set; }
            public string DisplayName { get; set; }
        }
    }
}
