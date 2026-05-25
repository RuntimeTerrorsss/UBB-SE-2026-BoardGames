// <copyright file="ServerStatusMessage.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace ServerCommunication
{
    public class ServerStatusMessage : MessageBase
    {
        public bool IsAvailable { get; set; }
    }
}
