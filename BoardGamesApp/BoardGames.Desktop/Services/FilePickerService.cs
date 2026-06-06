// <copyright file="FilePickerService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BoardGames.Desktop.Services
{
    public class FilePickerService : IFilePickerService
    {
        public async Task<string> PickImageFileAsync()
        {
            if (App.MainWindow == null)
            {
                return null;
            }

            FileOpenPicker fileOpenPicker = new FileOpenPicker();

            nint windowHandle = WindowNative.GetWindowHandle(App.MainWindow);
            InitializeWithWindow.Initialize(fileOpenPicker, windowHandle);

            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");

            StorageFile selectedFile = await fileOpenPicker.PickSingleFileAsync();

            return selectedFile?.Path;
        }
    }
}
