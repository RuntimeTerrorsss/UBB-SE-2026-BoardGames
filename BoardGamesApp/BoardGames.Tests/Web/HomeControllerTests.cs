//// <copyright file="HomeControllerTests.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using BoardGames.Web.Controllers;
//using Microsoft.AspNetCore.Mvc;
//using Xunit;

//namespace BoardGames.Tests.Web
//{
//    public class HomeControllerTests
//    {
//        [Fact]
//        public void Index_RedirectsToGamesIndex()
//        {
//            var controller = new HomeController();

//            var result = controller.Index() as RedirectToActionResult;

//            Assert.NotNull(result);
//            Assert.Equal("Games", result!.ControllerName);
//            Assert.Equal("Index", result.ActionName);
//        }

//        [Fact]
//        public void Privacy_ReturnsViewResult()
//        {
//            var controller = new HomeController();

//            var result = controller.Privacy();

//            Assert.IsType<ViewResult>(result);
//        }
//    }
//}
