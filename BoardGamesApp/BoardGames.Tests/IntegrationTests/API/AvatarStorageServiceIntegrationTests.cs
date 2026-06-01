// <copyright file="AvatarStorageServiceIntegrationTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BoardGames.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace BoardGames.Tests.IntegrationTests.Api
{
    [TestFixture]
    [Category("Integration")]
    public class AvatarStorageServiceIntegrationTests
    {
        private string tempDirectory = null!;
        private AvatarStorageService service = null!;

        [SetUp]
        public void Setup()
        {
            this.tempDirectory = Path.Combine(Path.GetTempPath(), $"avatar-tests-{Guid.NewGuid()}");

            var environmentMock = new Mock<IWebHostEnvironment>();
            environmentMock
                .Setup(environmentMock => environmentMock.ContentRootPath)
                .Returns(this.tempDirectory);

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();

            this.service = new AvatarStorageService(environmentMock.Object, configuration);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(this.tempDirectory))
            {
                Directory.Delete(this.tempDirectory, true);
            }
        }

        [Test]
        public async Task SaveAsync_WithValidExtension_SavesFile()
        {
            var accountId = Guid.NewGuid();
            var content = new MemoryStream(Encoding.UTF8.GetBytes("avatar-content"));

            var result = await this.service.SaveAsync(accountId, content, ".jpg");

            Assert.That(result, Does.Contain($"{accountId}.jpg"));

            string expectedPath = Path.Combine(
                this.tempDirectory,
                "Uploads",
                "Avatars",
                $"{accountId}.jpg");

            Assert.That(File.Exists(expectedPath), Is.True);
        }

        [Test]
        public async Task SaveAsync_WithExtensionWithoutDot_SavesNormalizedFile()
        {
            var accountId = Guid.NewGuid();
            var content = new MemoryStream(Encoding.UTF8.GetBytes("avatar"));

            var result = await this.service.SaveAsync(accountId, content, "png");

            Assert.That(result, Does.Contain(".png"));

            string expectedPath = Path.Combine(
                this.tempDirectory,
                "Uploads",
                "Avatars",
                $"{accountId}.png");

            Assert.That(File.Exists(expectedPath), Is.True);
        }

        [Test]
        public async Task SaveAsync_WithEmptyExtension_UsesDefaultPng()
        {
            var accountId = Guid.NewGuid();
            var content = new MemoryStream(Encoding.UTF8.GetBytes("avatar"));

            var result = await this.service.SaveAsync(accountId, content, string.Empty);

            Assert.That(result, Does.Contain(".png"));

            string expectedPath = Path.Combine(
                this.tempDirectory,
                "Uploads",
                "Avatars",
                $"{accountId}.png");

            Assert.That(File.Exists(expectedPath), Is.True);
        }

        [Test]
        public async Task SaveAsync_WithExistingAvatar_DeletesPreviousFile()
        {
            var accountId = Guid.NewGuid();

            await this.service.SaveAsync(
                accountId,
                new MemoryStream(Encoding.UTF8.GetBytes("old")),
                ".jpg");

            await this.service.SaveAsync(
                accountId,
                new MemoryStream(Encoding.UTF8.GetBytes("new")),
                ".png");

            string jpgPath = Path.Combine(
                this.tempDirectory,
                "Uploads",
                "Avatars",
                $"{accountId}.jpg");

            string pngPath = Path.Combine(
                this.tempDirectory,
                "Uploads",
                "Avatars",
                $"{accountId}.png");

            Assert.That(File.Exists(jpgPath), Is.False);
            Assert.That(File.Exists(pngPath), Is.True);
        }

        [Test]
        public async Task Delete_WithExistingFile_RemovesFile()
        {
            var accountId = Guid.NewGuid();

            string relativeUrl = await this.service.SaveAsync(
                accountId,
                new MemoryStream(Encoding.UTF8.GetBytes("avatar")),
                ".png");

            string absolutePath = Path.Combine(
                this.tempDirectory,
                "Uploads",
                "Avatars",
                $"{accountId}.png");

            Assert.That(File.Exists(absolutePath), Is.True);

            this.service.Delete(relativeUrl);

            Assert.That(File.Exists(absolutePath), Is.False);
        }

        [Test]
        [Obsolete]
        public void Delete_WithMissingFile_DoesNotThrow()
        {
            TestDelegate action = () =>
                this.service.Delete("/avatars/missing.png");

            Assert.DoesNotThrow(action);
        }

        [Test]
        [Obsolete]
        public void Delete_WithEmptyUrl_DoesNothing()
        {
            TestDelegate action = () =>
                this.service.Delete(string.Empty);

            Assert.DoesNotThrow(action);
        }

        [Test]
        [Obsolete]
        public void Delete_WithWhitespaceUrl_DoesNothing()
        {
            TestDelegate action = () =>
                this.service.Delete(" ");

            Assert.DoesNotThrow(action);
        }

        [Test]
        public async Task SaveAsync_ReturnsCorrectRelativeUrl()
        {
            var accountId = Guid.NewGuid();

            var result = await this.service.SaveAsync(
                accountId,
                new MemoryStream(Encoding.UTF8.GetBytes("avatar")),
                ".jpeg");

            Assert.That(result, Is.EqualTo($"/avatars/{accountId}.jpeg"));
        }
    }
}
