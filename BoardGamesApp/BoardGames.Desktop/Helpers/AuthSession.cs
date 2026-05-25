namespace BoardGames.Desktop.Helpers
{
    /// <summary>
    /// Keeps <see cref="SessionContext"/> and <see cref="SessionService"/> in sync for the desktop app.
    /// </summary>
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