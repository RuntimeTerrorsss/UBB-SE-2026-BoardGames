using Xunit;
// <copyright file="GeographicalServiceTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BoardGames.Tests.UnitTests
{
    public class GeographicalServiceTests : IDisposable
    {
        private readonly string _testAssetsFolder;
        private readonly string _testFilePath;

        public GeographicalServiceTests()
        {
            this._testAssetsFolder = Path.Combine(AppContext.BaseDirectory, "Assets");
            this._testFilePath = Path.Combine(this._testAssetsFolder, "RO.txt");
        }

        public void Dispose()
        {
            this.CleanUpTestFile();
        }

        private void SetupTestFile(string[] fileLines)
        {
            Directory.CreateDirectory(this._testAssetsFolder);
            File.WriteAllLines(this._testFilePath, fileLines);
        }

        private void CleanUpTestFile()
        {
            if (File.Exists(this._testFilePath))
            {
                File.Delete(this._testFilePath);
            }

            if (Directory.Exists(this._testAssetsFolder))
            {
                Directory.Delete(this._testAssetsFolder);
            }
        }

        #region LoadFromFileAsync & LoadCitiesFromFileAsync

        [Fact]
        public async Task LoadFromFileAsync_FileDoesNotExist_ThrowsInvalidOperationException()
        {
            this.CleanUpTestFile();

            await Assert.ThrowsAsync<InvalidOperationException>(() => GeographicalService.LoadFromFileAsync());
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_LineHasInsufficientColumns_LineIsSkipped()
        {
            var lines = new[] { "Column1\tColumn2" };
            this.SetupTestFile(lines);

            var service = new GeographicalService();

            await service.LoadCitiesFromFileAsync();

            var details = service.GetCityDetails("Column2");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_InvalidFeatureClass_LineIsSkipped()
        {
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tX\t\t\t\t\t\t\t\t100000";
            this.SetupTestFile(new[] { line });

            var service = new GeographicalService();

            await service.LoadCitiesFromFileAsync();

            var details = service.GetCityDetails("Cluj");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_PopulationBelowMinimum_LineIsSkipped()
        {
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t4999";
            this.SetupTestFile(new[] { line });

            var service = new GeographicalService();

            await service.LoadCitiesFromFileAsync();

            var details = service.GetCityDetails("Cluj");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_InvalidLatitude_LineIsSkipped()
        {
            var line = "Id\tCluj\tCluj\t\tINVALID\t23.62\tP\t\t\t\t\t\t\t\t6000";
            this.SetupTestFile(new[] { line });

            var service = new GeographicalService();

            await service.LoadCitiesFromFileAsync();

            var details = service.GetCityDetails("Cluj");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_InvalidLongitude_LineIsSkipped()
        {
            var line = "Id\tCluj\tCluj\t\t46.77\tINVALID\tP\t\t\t\t\t\t\t\t6000";
            this.SetupTestFile(new[] { line });

            var service = new GeographicalService();

            await service.LoadCitiesFromFileAsync();

            var details = service.GetCityDetails("Cluj");
            Assert.False(details.IsFound);
        }

        [Fact]
        public async Task LoadCitiesFromFileAsync_ValidBucuresti_AppendsSpecialBucharestAliases()
        {
            var line = "Id\tBucuresti\tBucuresti\tBuc,B-est\t44.42\t26.10\tPPLC\t\t\t\t\t\t\t\t2000000";
            this.SetupTestFile(new[] { line });

            var service = new GeographicalService();

            await service.LoadCitiesFromFileAsync();

            Assert.True(service.GetCityDetails("Bucuresti").IsFound);
            Assert.True(service.GetCityDetails("Bucharest").IsFound);
            Assert.True(service.GetCityDetails("București").IsFound);
            Assert.True(service.GetCityDetails("Buc").IsFound);
            Assert.True(service.GetCityDetails("B est").IsFound);
        }

        #endregion

        #region GetCityDetails

        [Fact]
        public async Task GetCityDetails_CityNotFound_ReturnsFalseAndDefaultCoordinates()
        {
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            this.SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            var result = service.GetCityDetails("NonExistentCity");

            Assert.False(result.IsFound);
            Assert.Equal("", result.CityName);
            Assert.Equal(0, result.Latitude);
            Assert.Equal(0, result.Longitude);
        }

        [Fact]
        public async Task GetCityDetails_CityFoundWithNormalization_ReturnsTrueAndCorrectDetails()
        {
            var line = "Id\tSânnicolau-Mare\tSannicolau-Mare\t\t46.07\t20.62\tP\t\t\t\t\t\t\t\t12000";
            this.SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            var result = service.GetCityDetails("sânnicolau mare");

            Assert.True(result.IsFound);
            Assert.Equal("Sânnicolau-Mare", result.CityName);
            Assert.Equal(46.07, result.Latitude);
            Assert.Equal(20.62, result.Longitude);
        }

        #endregion

        #region GetDistanceBetweenCities

        [Fact]
        public async Task GetDistanceBetweenCities_OriginNotFound_ReturnsNull()
        {
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            this.SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            var result = service.GetDistanceBetweenCities("MissingCity", "Cluj");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDistanceBetweenCities_DestinationNotFound_ReturnsNull()
        {
            var line = "Id\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            this.SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            var result = service.GetDistanceBetweenCities("Cluj", "MissingCity");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDistanceBetweenCities_BothCitiesExist_ReturnsCalculatedDistance()
        {
            var line1 = "Id1\tCluj\tCluj\t\t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            var line2 = "Id2\tOradea\tOradea\t\t47.04\t21.91\tP\t\t\t\t\t\t\t\t200000";
            this.SetupTestFile(new[] { line1, line2 });
            var service = await GeographicalService.LoadFromFileAsync();

            var result = service.GetDistanceBetweenCities("Cluj", "Oradea");

            Assert.NotNull(result);
            Assert.True(result > 0);
        }

        #endregion

        #region GetCitySuggestions

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetCitySuggestions_NullOrWhiteSpaceInput_ReturnsEmptyList(string input)
        {
            var service = new GeographicalService();

            var result = service.GetCitySuggestions(input);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCitySuggestions_MatchesFound_ReturnsNormalizedSuggestionsUpToMaximumLimit()
        {
            var lines = new List<string>();
            for (int i = 1; i <= 12; i++)
            {
                lines.Add($"Id{i}\tTestCity{i}\tTestCity{i}\t\t46.0\t23.0\tP\t\t\t\t\t\t\t\t6000");
            }

            this.SetupTestFile(lines.ToArray());
            var service = await GeographicalService.LoadFromFileAsync();

            var result = service.GetCitySuggestions("Test");

            Assert.Equal(10, result.Count);
            Assert.Contains("TestCity1", result);
        }

        [Fact]
        public async Task GetCitySuggestions_BucharestAliasInput_NormalizesSuggestionsToBucuresti()
        {
            var line = "Id\tBucharest\tBucharest\t\t44.42\t26.10\tPPLC\t\t\t\t\t\t\t\t2000000";
            this.SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            var result = service.GetCitySuggestions("Bucha");

            Assert.Single(result);
            Assert.Equal("București", result[0]);
        }

        [Fact]
        public async Task GetCitySuggestions_BucurestiNameInput_NormalizesSuggestionsToBucurestiWithDiacritics()
        {
            var line = "Id\tBucuresti\tBucuresti\t\t44.42\t26.10\tPPLC\t\t\t\t\t\t\t\t2000000";
            this.SetupTestFile(new[] { line });
            var service = await GeographicalService.LoadFromFileAsync();

            var result = service.GetCitySuggestions("Bucur");

            Assert.Single(result);
            Assert.Equal("București", result[0]);
        }

        #endregion

        #region Normalization and Aliasing

        [Fact]
        public async Task LoadCitiesFromFileAsync_AlternateNamesHasEmptyElements_TriggersAddCityAliasEarlyReturn()
        {
            var line = "Id\tCluj\tCluj\tCluj, , \t46.77\t23.62\tP\t\t\t\t\t\t\t\t100000";
            this.SetupTestFile(new[] { line });
            var service = new GeographicalService();

            var exception = await Record.ExceptionAsync(() => service.LoadCitiesFromFileAsync());

            Assert.Null(exception);
            Assert.True(service.GetCityDetails("Cluj").IsFound);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetCityDetails_NullOrWhiteSpaceInput_TriggersNormalizeCityNameEarlyReturn(string invalidCityName)
        {
            var service = new GeographicalService();

            var result = service.GetCityDetails(invalidCityName);

            Assert.False(result.IsFound);
            Assert.Equal(string.Empty, result.CityName);
        }

        #endregion
    }
}
