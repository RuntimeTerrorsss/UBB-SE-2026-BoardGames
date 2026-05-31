using System.Globalization;

namespace BoardGames.Api.Legacy.Services
{
    public class GeographicalService : InterfaceGeographicalService
    {
        private const int MinimumCityPopulation = 5000;
        private const int MinimumRequiredColumns = 15;
        private const string FeatureClassPopulatedPlace = "P";
        private const string FeatureClassCapitalCity = "PPLC";
        private const double DefaultCoordinateValue = 0;
        private const string EmptyCityName = "";
        private const int MaximumCitySuggestions = 10;

        private const int ColumnIndexName = 1;
        private const int ColumnIndexAsciiName = 2;
        private const int ColumnIndexAlternateNames = 3;
        private const int ColumnIndexLatitude = 4;
        private const int ColumnIndexLongitude = 5;
        private const int ColumnIndexFeatureClass = 6;
        private const int ColumnIndexPopulation = 14;
        private readonly Dictionary<string, City> cityLookupByNormalizedName = new();
        public GeographicalService()
        {
        }
        public static async Task<GeographicalService> LoadFromFileAsync()
        {
            var service = new GeographicalService();
            await service.LoadCitiesFromFileAsync();
            return service;
        }
        public async Task LoadCitiesFromFileAsync()
        {
            var lines = await ReadRoCityLinesAsync();

            foreach (var line in lines)
            {
                var columns = line.Split('\t');

                if (columns.Length < MinimumRequiredColumns)
                {
                    continue;
                }

                var featureClass = columns[ColumnIndexFeatureClass];
                if (featureClass != FeatureClassPopulatedPlace && featureClass != FeatureClassCapitalCity)
                {
                    continue;
                }

                long.TryParse(columns[ColumnIndexPopulation], out var population);
                if (population < MinimumCityPopulation)
                {
                    continue;
                }

                var primaryCityName = columns[ColumnIndexName];
                var asciiCityName = columns[ColumnIndexAsciiName];
                var alternateCityNames = columns[ColumnIndexAlternateNames];

                if (!double.TryParse(columns[ColumnIndexLatitude], NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude))
                {
                    continue;
                }

                if (!double.TryParse(columns[ColumnIndexLongitude], NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
                {
                    continue;
                }

                var city = new City
                {
                    MainName = primaryCityName,
                    Latitude = latitude,
                    Longitude = longitude,
                    Names = new List<string>(),
                };

                this.AddCityAlias(city, primaryCityName);
                this.AddCityAlias(city, asciiCityName);

                if (asciiCityName.Trim().Equals("Bucuresti", StringComparison.OrdinalIgnoreCase))
                {
                    this.AddCityAlias(city, "Bucharest");
                    this.AddCityAlias(city, "BucureÃˆâ„¢ti");
                }

                if (!string.IsNullOrWhiteSpace(alternateCityNames))
                {
                    foreach (var alternate in alternateCityNames.Split(','))
                    {
                        this.AddCityAlias(city, alternate);
                    }
                }
            }
        }

        private static async Task<IReadOnlyList<string>> ReadRoCityLinesAsync()
        {
            string relativePath = Path.Combine("Assets", "RO.txt");
            string nextToExecutable = Path.Combine(AppContext.BaseDirectory, relativePath);
            if (File.Exists(nextToExecutable))
            {
                return await File.ReadAllLinesAsync(nextToExecutable);
            }

            try
            {
                throw new FileNotFoundException($"Could not find Assets/RO.txt at {nextToExecutable}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Could not load Assets/RO.txt from the output directory or from the application package. Ensure RO.txt is listed as Content in BoardGames.csproj.",
                    ex);
            }
        }
        public (bool IsFound, string CityName, double Latitude, double Longitude) GetCityDetails(string cityName)
        {
            var normalizedCityName = this.NormalizeCityName(cityName);

            if (this.cityLookupByNormalizedName.TryGetValue(normalizedCityName, out var city))
            {
                return (true, city.MainName, city.Latitude, city.Longitude);
            }

            return (false, EmptyCityName, DefaultCoordinateValue, DefaultCoordinateValue);
        }
        public double? GetDistanceBetweenCities(string originCityName, string destinationCityName)
        {
            var originCityDetails = this.GetCityDetails(originCityName);
            var destinationCityDetails = this.GetCityDetails(destinationCityName);

            if (!originCityDetails.IsFound || !destinationCityDetails.IsFound)
            {
                return null;
            }

            return GeographicDistance.CalculateDistance(
                originCityDetails.Latitude,
                originCityDetails.Longitude,
                destinationCityDetails.Latitude,
                destinationCityDetails.Longitude);
        }
        public List<string> GetCitySuggestions(string partialName)
        {
            if (string.IsNullOrWhiteSpace(partialName))
            {
                return new List<string>();
            }

            var normalizedPartialName = this.NormalizeCityName(partialName);
            var rawSuggestions = this.cityLookupByNormalizedName
                .Where(cityLookupEntry => cityLookupEntry.Key.Contains(normalizedPartialName))
                .Select(cityLookupEntry => cityLookupEntry.Value.MainName)
                .Distinct();

            var normalizedSuggestions = rawSuggestions
                .Select(this.NormalizeSuggestion)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(MaximumCitySuggestions)
                .ToList();

            return normalizedSuggestions;
        }

        private string NormalizeSuggestion(string suggestion)
        {
            if (string.Equals(suggestion, "Bucharest", StringComparison.OrdinalIgnoreCase))
            {
                return "BucureÃˆâ„¢ti";
            }

            if (string.Equals(suggestion, "Bucuresti", StringComparison.OrdinalIgnoreCase))
            {
                return "BucureÃˆâ„¢ti";
            }

            return suggestion;
        }

        private void AddCityAlias(City city, string originalCityName)
        {
            var normalizedcityname = this.NormalizeCityName(originalCityName);

            if (string.IsNullOrWhiteSpace(normalizedcityname))
            {
                return;
            }

            city.Names.Add(normalizedcityname);

            if (!this.cityLookupByNormalizedName.ContainsKey(normalizedcityname))
            {
                this.cityLookupByNormalizedName[normalizedcityname] = city;
            }
        }

        private string NormalizeCityName(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return string.Empty;
            }

            return city
                .Trim()
                .ToLower()
                .Replace("-", " ")
                .Replace("Ã„Æ’", "a")
                .Replace("ÃƒÂ¢", "a")
                .Replace("ÃƒÂ®", "i")
                .Replace("Ãˆâ„¢", "s")
                .Replace("Ã…Â£", "t")
                .Replace("Ãˆâ€º", "t");
        }
    }
}
