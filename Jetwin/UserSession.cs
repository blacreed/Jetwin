namespace Jetwin
{
    public static class UserSession
    {
        public static int StaffID { get; private set; }
        public static string Username { get; private set; }
        public static string Role { get; private set; }

        public static void SetUser(int staffID, string username, string role)
        {
            StaffID = staffID;
            Username = username;
            Role = role;
        }

        public static void ClearSession()
        {
            StaffID = 0;
            Username = null;
            Role = null;
        }
    }

}
