namespace BoardGames.Api.Services
{
    public interface IAvatarStorageService
    {
        Task<string> SaveAsync(Guid accountId, Stream content, string fileExtension);

        void Delete(string relativeUrl);
    }
}
