namespace BoardGames.Desktop.Services
{
    public interface IFilePickerService
    {
        Task<string> PickImageFileAsync();
    }
}
