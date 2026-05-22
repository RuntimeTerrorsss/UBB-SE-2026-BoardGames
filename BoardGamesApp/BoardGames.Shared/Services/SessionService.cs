using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardGames.Shared.Services
{
    public class SessionService
    {
        public int? UserId { get; private set; }
        public string? Username { get; private set; }
        public string? DisplayName { get; private set; }
        public bool IsLoggedIn => UserId.HasValue;

        public void SetUser(int id, string username, string displayName)
        {
            UserId = id;
            Username = username;
            DisplayName = displayName;
        }

        public void Clear()
        {
            UserId = null;
            Username = null;
            DisplayName = null;
        }
    }
}
