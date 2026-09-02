namespace CrudApp.Services
{
    public static class Session
    {
        public static int UserId { get; private set; }
        public static string? CurrentUsername { get; private set; }
        public static string? CurrentUserRole { get; private set; }
        public static bool IsLoggedIn { get; private set; }

        public static void Login(int userId, string username, string role)
        {
            UserId = userId;
            CurrentUsername = username;
            CurrentUserRole = role;
            IsLoggedIn = true;
        }

        public static void Logout()
        {
            UserId = 0;
            CurrentUsername = null;
            CurrentUserRole = null;
            IsLoggedIn = false;
        }
    }
}
