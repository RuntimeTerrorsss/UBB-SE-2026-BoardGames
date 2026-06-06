namespace BoardGames.Desktop.Helpers
{
    public static class AuthSession
    {
        public static bool IsLoggedIn =>
            SessionContext.GetInstance().IsLoggedIn && SessionContext.GetInstance().UserId > 0;

        public static void SetAuthenticatedUser(User user, SessionService sessionService)
        {
            sessionService.SetUser(user.Id, user.Username, user.DisplayName);
            SessionContext.GetInstance().Populate(user);
        }

        public static void Clear(SessionService sessionService)
        {
            sessionService.Clear();
            SessionContext.GetInstance().Clear();
        }
    }
}