//// <copyright file="GameImageHelperTests.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System;
//using BoardGames.Shared.DTO;
//using BoardGames.Web.Helpers;
//using Xunit;

//namespace BoardGames.Tests.Web
//{
//    public class GameImageHelperTests
//    {
//        [Fact]
//        public void GetDisplaySource_GameSummaryDTO_UsesImageUrl()
//        {
//            var game = new GameSummaryDTO
//            {
//                ImageUrl = "https://cdn.example.com/catan.png",
//            };

//            string source = GameImageHelper.GetDisplaySource(game);

//            Assert.Equal("https://cdn.example.com/catan.png", source);
//        }

//        [Fact]
//        public void GetDisplaySource_GameSummaryDTOWithoutUrl_ReturnsPlaceholder()
//        {
//            string source = GameImageHelper.GetDisplaySource(new GameSummaryDTO());

//            Assert.Equal(GameImageHelper.PlaceholderImageSource, source);
//        }

//        [Fact]
//        public void GetDisplaySource_GameDTO_PrefersUploadedBytes()
//        {
//            var game = new GameDTO
//            {
//                Image = new byte[] { 0x89, 0x50, 0x4E, 0x47 },
//                ImageUrl = "https://cdn.example.com/ignored.png",
//            };

//            string source = GameImageHelper.GetDisplaySource(game);

//            Assert.StartsWith("data:image/png;base64,", source, StringComparison.Ordinal);
//        }
//    }
//}
