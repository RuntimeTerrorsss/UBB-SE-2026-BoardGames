namespace BoardGames.Api.Services
{
    public class SessionService
    {
        public int? UserId { get; private set; }

        public string? Username { get; private set; }

        public string? DisplayName { get; private set; }

        public bool IsLoggedIn => this.UserId.HasValue;

        public void SetUser(int id, string username, string displayName)
        {
            this.UserId = id;
            this.Username = username;
            this.DisplayName = displayName;
        }

        public void Clear()
        {
            this.UserId = null;
            this.Username = null;
            this.DisplayName = null;
        }
    }
}
