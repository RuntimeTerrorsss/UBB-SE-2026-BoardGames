namespace BoardGames.Api.Legacy.Services
{
    public interface IMapService
    {
        public Task<Address?> GetAddressFromMapAsync(double latitude, double longitude);
    }
}
