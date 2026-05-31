// <copyright file="SearchControllerTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;
using BoardGames.Web.Controllers;
using BoardGames.Web.Infrastructure;
using BoardGames.Web.Models.Search;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace BoardGames.Tests.Web
{
    public class SearchControllerTests
    {
        [Fact]
        public async Task Filter_InvalidDateRange_SetsErrorMessage()
        {
            var gameProxy = new Mock<IGameProxyService>();
            var controller = new SearchController(gameProxy.Object);
            var model = new SearchFilterViewModel
            {
                StartDate = DateTime.Today.AddDays(3),
                EndDate = DateTime.Today,
            };

            var result = await controller.Filter(model) as ViewResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result!.ViewName);
            var viewModel = Assert.IsType<SearchFilterViewModel>(result.Model);
            Assert.Equal("Start date must be before end date.", viewModel.ErrorMessage);
            gameProxy.Verify(
                service => service.SearchGamesAsync(It.IsAny<GameSearchCriteriaDTO>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Filter_ValidCriteria_CallsProxyAndPaginates()
        {
            var games = Enumerable.Range(1, 8).Select(index => new GameDTO
            {
                Id = index,
                Name = $"Game {index}",
                Price = 10m,
                City = "Cluj",
                MinimumPlayerNumber = 2,
                MaximumPlayerNumber = 4,
            }).ToList();

            var gameProxy = new Mock<IGameProxyService>();
            gameProxy
                .Setup(service => service.SearchGamesAsync(It.IsAny<GameSearchCriteriaDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games);

            var controller = new SearchController(gameProxy.Object);
            var model = new SearchFilterViewModel { Page = 1, PageSize = 3 };

            var result = await controller.Filter(model) as ViewResult;

            Assert.NotNull(result);
            var viewModel = Assert.IsType<SearchFilterViewModel>(result!.Model);
            Assert.Equal(3, viewModel.Results.Count);
            Assert.Equal(3, viewModel.TotalPages);
            gameProxy.Verify(
                service => service.SearchGamesAsync(It.IsAny<GameSearchCriteriaDTO>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Filter_NoResults_ReturnsEmptyList()
        {
            var gameProxy = new Mock<IGameProxyService>();
            gameProxy
                .Setup(service => service.SearchGamesAsync(It.IsAny<GameSearchCriteriaDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GameDTO>());

            var controller = new SearchController(gameProxy.Object);
            var model = new SearchFilterViewModel { Page = 1, PageSize = 5 };

            var result = await controller.Filter(model) as ViewResult;

            Assert.NotNull(result);
            var viewModel = Assert.IsType<SearchFilterViewModel>(result!.Model);
            Assert.Empty(viewModel.Results);
            Assert.Equal(0, viewModel.TotalPages);
        }

        [Fact]
        public async Task Filter_WithNameFilter_PassesNameToProxy()
        {
            var gameProxy = new Mock<IGameProxyService>();
            gameProxy
                .Setup(service => service.SearchGamesAsync(It.IsAny<GameSearchCriteriaDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GameDTO>());

            var controller = new SearchController(gameProxy.Object);
            var model = new SearchFilterViewModel { Name = "Chess", Page = 1, PageSize = 5 };

            await controller.Filter(model);

            gameProxy.Verify(
                service => service.SearchGamesAsync(
                    It.Is<GameSearchCriteriaDTO>(criteria => criteria.Name == "Chess"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Filter_WithMaxPrice_PassesMaxPriceToProxy()
        {
            var gameProxy = new Mock<IGameProxyService>();
            gameProxy
                .Setup(service => service.SearchGamesAsync(It.IsAny<GameSearchCriteriaDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GameDTO>());

            var controller = new SearchController(gameProxy.Object);
            var model = new SearchFilterViewModel { MaximumPrice = 50m, Page = 1, PageSize = 5 };

            await controller.Filter(model);

            gameProxy.Verify(
                service => service.SearchGamesAsync(
                    It.Is<GameSearchCriteriaDTO>(criteria => criteria.MaximumPrice == 50m),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Filter_SecondPage_ReturnsCorrectSlice()
        {
            var games = Enumerable.Range(1, 9).Select(index => new GameDTO
            {
                Id = index,
                Name = $"Game {index}",
            }).ToList();

            var gameProxy = new Mock<IGameProxyService>();
            gameProxy
                .Setup(service => service.SearchGamesAsync(It.IsAny<GameSearchCriteriaDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(games);

            var controller = new SearchController(gameProxy.Object);
            var model = new SearchFilterViewModel { Page = 2, PageSize = 3 };

            var result = await controller.Filter(model) as ViewResult;

            Assert.NotNull(result);
            var viewModel = Assert.IsType<SearchFilterViewModel>(result!.Model);
            Assert.Equal(3, viewModel.Results.Count);
            Assert.Equal(4, viewModel.Results[0].Id);
        }
    }
}
