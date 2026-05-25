using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using Moq;
using Moq.Protected;
using Xunit;


namespace BoardGames.Tests.UnitTests
{
    public class MapServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;

        public MapServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        }

        #region Constructor Logic

        [Fact]
        public void MapService_ParameterlessConstructor_InitializesCorrectly()
        {



            var service = new MapService();



            Assert.NotNull(service);
        }

        [Fact]
        public void MapService_ConstructorWithClient_SetsUserAgentHeader()
        {

            var service = new MapService(_httpClient);


            Assert.True(_httpClient.DefaultRequestHeaders.Contains("User-Agent"));
            var userAgent = _httpClient.DefaultRequestHeaders.UserAgent.ToString();
            Assert.Equal("BookingBoardgames/1.0", userAgent);
        }

        #endregion

        #region GetAddressFromMapAsync - Edge Cases & Failures

        [Fact]
        public async Task GetAddressFromMapAsync_DefaultCoordinates_ReturnsNull()
        {

            var service = new MapService(_httpClient);


            var result = await service.GetAddressFromMapAsync(0.0, 0.0);


            Assert.Null(result);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_HttpErrorResponse_ReturnsNull()
        {

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var service = new MapService(_httpClient);


            var result = await service.GetAddressFromMapAsync(46.77, 23.62);


            Assert.Null(result);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_InvalidJsonResponse_ReturnsNull()
        {

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ invalid json }")
                });

            var service = new MapService(_httpClient);


            var result = await service.GetAddressFromMapAsync(46.77, 23.62);


            Assert.Null(result);
        }

        #endregion

        #region GetAddressFromMapAsync - Address Type Selection Hierarchy

        [Fact]
        public async Task GetAddressFromMapAsync_HasCity_SelectsCity()
        {

            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": ""Romania"",
                    ""city"": ""Cluj-Napoca"",
                    ""town"": ""ClujTown"",
                    ""village"": ""ClujVillage"",
                    ""road"": ""Strada Universitatii"",
                    ""house_number"": ""7""
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);


            var result = await service.GetAddressFromMapAsync(46.77, 23.62);


            Assert.NotNull(result);
            Assert.Equal("Romania", result.Country);
            Assert.Equal("Cluj-Napoca", result.City);
            Assert.Equal("Strada Universitatii", result.Street);
            Assert.Equal("7", result.StreetNumber);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_NoCityHasTown_SelectsTown()
        {

            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": ""Romania"",
                    ""town"": ""Floresti"",
                    ""village"": ""FlorestiVillage"",
                    ""road"": ""Strada Avram Iancu""
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);


            var result = await service.GetAddressFromMapAsync(46.72, 23.52);


            Assert.NotNull(result);
            Assert.Equal("Floresti", result.City);
            Assert.Equal(string.Empty, result.StreetNumber);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_NoCityNoTownHasVillage_SelectsVillage()
        {

            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": ""Romania"",
                    ""village"": ""Chinteni""
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);


            var result = await service.GetAddressFromMapAsync(46.85, 23.53);


            Assert.NotNull(result);
            Assert.Equal("Chinteni", result.City);
            Assert.Equal(string.Empty, result.Street);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_NoCityNoTownNoVillage_ReturnsEmptyStringForCity()
        {

            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": ""Romania""
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);


            var result = await service.GetAddressFromMapAsync(46.00, 23.00);


            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.City);
        }

        [Fact]
        public async Task GetAddressFromMapAsync_PropertiesAreNullInJson_ReturnsEmptyStrings()
        {

            var jsonResponse = @"
            {
                ""address"": {
                    ""country"": null,
                    ""city"": null,
                    ""road"": null,
                    ""house_number"": null
                }
            }";

            SetupMockHttpMessageHandler(jsonResponse);
            var service = new MapService(_httpClient);


            var result = await service.GetAddressFromMapAsync(46.00, 23.00);


            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.Country);
            Assert.Equal(string.Empty, result.City);
            Assert.Equal(string.Empty, result.Street);
            Assert.Equal(string.Empty, result.StreetNumber);
        }

        #endregion

        #region Helper Methods

        private void SetupMockHttpMessageHandler(string jsonResponse)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });
        }

        #endregion
    }
}