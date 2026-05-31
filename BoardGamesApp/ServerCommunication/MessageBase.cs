// <copyright file="MessageBase.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Text.Json;

namespace ServerCommunication
{
    public abstract class MessageBase
    {
        public MessageWrapper ToMessageWrapper()
        {
            return new MessageWrapper
            {
                Type = this.GetType().Name,
                Payload = JsonSerializer.SerializeToUtf8Bytes((object)this),
            };
        }
    }
}
