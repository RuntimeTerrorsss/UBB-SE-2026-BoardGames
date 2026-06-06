using System.Diagnostics;
using System.Text.Json;

namespace BoardGames.Api.Legacy.Services
{
    public class MapService : IMapService
    {
        private const string NominatimUrlTemplate = "https://nominatim.openstreetmap.org/reverse?lat={0}&lon={1}&format=json";
        private const string UserAgentValue = "BoardGames/1.0";
        private const double DefaultCoordinate = 0.0;
        private readonly HttpClient httpClient;

        public MapService()
            : this(new HttpClient())
        {
        }

        public MapService(HttpClient client)
        {
            this.httpClient = client;
            if (!this.httpClient.DefaultRequestHeaders.Contains("User-Agent"))
            {
                this.httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgentValue);
            }
        }

        public async Task<Address?> GetAddressFromMapAsync(double latitude, double longitude)
        {
            if (latitude == DefaultCoordinate && longitude == DefaultCoordinate)
            {
                return null;
            }

            try
            {
                string requestUrl = string.Format(System.Globalization.CultureInfo.InvariantCulture, NominatimUrlTemplate, latitude, longitude);
                HttpResponseMessage response = await this.httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                using (JsonDocument jsonDocument = JsonDocument.Parse(jsonResponse))
                {
                    JsonElement address = jsonDocument.RootElement.GetProperty("address");

                    return new Address
                    {
                        Country = address.TryGetProperty("country", out JsonElement country) ? country.GetString() ?? string.Empty : string.Empty,
                        City = address.TryGetProperty("city", out JsonElement city) ? city.GetString() ?? string.Empty
                                     : address.TryGetProperty("town", out JsonElement town) ? town.GetString() ?? string.Empty
                                     : address.TryGetProperty("village", out JsonElement village) ? village.GetString() ?? string.Empty : string.Empty,
                        Street = address.TryGetProperty("road", out JsonElement road) ? road.GetString() ?? string.Empty : string.Empty,
                        StreetNumber = address.TryGetProperty("house_number", out JsonElement number) ? number.GetString() ?? string.Empty : string.Empty,
                    };
                }
            }
            catch (Exception exception)
            {
                Debug.Write($"Error in MapService: {exception.Message}");
                return null;
            }
        }
    }
}
