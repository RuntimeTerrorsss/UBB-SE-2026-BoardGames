// <copyright file="SubscribeToServerMessage.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace ServerCommunication
{
    public class SubscribeToServerMessage : MessageBase
    {
        public int UserId { get; set; }
    }
}
