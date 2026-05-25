// <copyright file="IAvatarStorageService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Api.Services
{
    public interface IAvatarStorageService
    {
        Task<string> SaveAsync(Guid accountId, Stream content, string fileExtension);

        void Delete(string relativeUrl);
    }
}
