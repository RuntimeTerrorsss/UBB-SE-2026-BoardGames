using System;
using System.Collections.Generic;

namespace BoardRentAndProperty
{
    public sealed class NotificationManager
    {
        public event EventHandler<IDictionary<string, string>>? NotificationClicked;

        public void Init()
        {
        }

        public void Unregister()
        {
        }
    }
}
