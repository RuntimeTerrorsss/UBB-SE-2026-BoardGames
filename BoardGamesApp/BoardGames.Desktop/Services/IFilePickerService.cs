// <copyright file="IFilePickerService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

namespace BoardGames.Desktop.Services
{
    public interface IFilePickerService
    {
        Task<string> PickImageFileAsync();
    }
}
