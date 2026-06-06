namespace BoardGames.Api.Legacy.Services
{
    public interface InterfaceGeographicalService
    {
        Task LoadCitiesFromFileAsync();
        (bool IsFound, string CityName, double Latitude, double Longitude) GetCityDetails(string cityName);
        double? GetDistanceBetweenCities(string originCity, string destinationCity);
        public List<string> GetCitySuggestions(string partialName);
    }
}
