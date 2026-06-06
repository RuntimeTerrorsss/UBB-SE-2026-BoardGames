using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using Moq;
using BoardGames.Data;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Api.Controllers;
using BoardGames.Api.Services;
using BoardGames.Api.Mappers;
using BoardGames.Shared.DTO;
using BoardGames.Shared.Common;

namespace BoardGames.WebTests.IntegrationTests
{
    [Collection("SharedDatabase")]
    public class ProfileAvatarIntegrationTests
    {
        private readonly SharedDatabaseFixture _fixture;

        public ProfileAvatarIntegrationTests(SharedDatabaseFixture fixture)
        {
            this._fixture = fixture;
        }

        private async Task<(AccountsController, AppDbContext, string)> CreateTestSubject(IServiceScope scope)
        {
            var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var dbContext = dbContextFactory.CreateDbContext();

            // Create a unique temporary directory for physical file upload tests
            string testFolder = "AvatarsTest_" + Guid.NewGuid().ToString("N");
            string fullTempPath = Path.Combine(Path.GetTempPath(), testFolder);
            
            var mockEnv = new Mock<IWebHostEnvironment>();
            mockEnv.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());

            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["Storage:AvatarFolder"]).Returns(testFolder);
            mockConfig.Setup(c => c["Storage:AvatarUrlPrefix"]).Returns("/avatars");

            var avatarStorage = new AvatarStorageService(mockEnv.Object, mockConfig.Object);

            var accountRepo = new AccountRepository(dbContextFactory);
            var failedLoginRepo = new FailedLoginRepository(dbContextFactory);
            var mapper = new AccountProfileMapper();

            var accountService = new AccountService(accountRepo, mapper, avatarStorage, failedLoginRepo);
            var controller = new AccountsController(accountService, avatarStorage);

            return (controller, dbContext, fullTempPath);
        }

        private static IFormFile CreateMockFormFile(string fileName, byte[] content)
        {
            var stream = new MemoryStream(content);
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.Length).Returns(content.Length);
            return mockFile.Object;
        }

        [Fact]
        public async Task UploadAvatar_ExceedsSizeLimit_ReturnsBadRequest()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (controller, dbContext, tempFolder) = await this.CreateTestSubject(scope);

            var random = new Random();
            var user = new User { Id = Guid.NewGuid(), PamUserId = random.Next(100000, 999999), Username = "oversize_user", Email = "oversize@test.com", PasswordHash = "hash", Country = "Test", City = "Test" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // 5 MB + 1 byte
            var largeContent = new byte[(5 * 1024 * 1024) + 1]; 
            var mockFile = CreateMockFormFile("large.png", largeContent);

            // Act
            var result = await controller.UploadAvatar(user.Id, mockFile);

            // Assert
            var badRequest = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(400, badRequest.StatusCode);
            var errorResponse = Assert.IsType<ApiErrorResponse>(badRequest.Value);
            Assert.Contains("Maximum allowed size is 5 MB", errorResponse.Error);
            
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }

        [Fact]
        public async Task UploadAvatar_Success_WritesToDiskAndSyncsToDatabase()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (controller, dbContext, tempFolder) = await this.CreateTestSubject(scope);

            var random = new Random();
            var user = new User { Id = Guid.NewGuid(), PamUserId = random.Next(100000, 999999), Username = "avatar_user", Email = "avatar@test.com", PasswordHash = "hash", Country = "Test", City = "Test" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            var fileContent = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG magic numbers
            var mockFile = CreateMockFormFile("my_avatar.png", fileContent);

            try
            {
                // Act
                var result = await controller.UploadAvatar(user.Id, mockFile);

                // Assert
                var okResult = Assert.IsType<OkObjectResult>(result.Result);
                var responseDto = Assert.IsType<AvatarUploadResponseDTO>(okResult.Value);
                
                string expectedRelativeUrl = $"/avatars/{user.Id}.png";
                Assert.Equal(expectedRelativeUrl, responseDto.AvatarUrl);

                // Verify file physically exists on disk
                string expectedPhysicalPath = Path.Combine(tempFolder, $"{user.Id}.png");
                Assert.True(File.Exists(expectedPhysicalPath), "Physical avatar file was not saved to disk.");

                // Verify DB sync
                var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
                var updatedUser = await finalDb.Users.FirstAsync(u => u.Id == user.Id);
                Assert.Equal(expectedRelativeUrl, updatedUser.AvatarUrl);
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }

        [Fact]
        public async Task UploadAvatar_OverwritesOldAvatar_CleansUpOldFile()
        {
            // Arrange
            using var scope = this._fixture.Factory.Services.CreateScope();
            var (controller, dbContext, tempFolder) = await this.CreateTestSubject(scope);

            var random = new Random();
            var user = new User { Id = Guid.NewGuid(), PamUserId = random.Next(100000, 999999), Username = "replace_user", Email = "replace@test.com", PasswordHash = "hash", Country = "Test", City = "Test" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            try
            {
                // Initial Upload (.png)
                var firstFile = CreateMockFormFile("old.png", new byte[] { 1, 2, 3 });
                await controller.UploadAvatar(user.Id, firstFile);
                
                string firstPhysicalPath = Path.Combine(tempFolder, $"{user.Id}.png");
                Assert.True(File.Exists(firstPhysicalPath), "First avatar file should exist.");

                // Act - Upload new avatar with different extension (.jpg)
                var newFile = CreateMockFormFile("new.jpg", new byte[] { 4, 5, 6 });
                await controller.UploadAvatar(user.Id, newFile);

                // Assert
                string newPhysicalPath = Path.Combine(tempFolder, $"{user.Id}.jpg");
                
                Assert.True(File.Exists(newPhysicalPath), "New avatar file should exist.");
                Assert.False(File.Exists(firstPhysicalPath), "Old avatar file should have been deleted by AvatarStorageService.");

                // Verify DB was updated
                var finalDb = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();
                var updatedUser = await finalDb.Users.FirstAsync(u => u.Id == user.Id);
                Assert.Equal($"/avatars/{user.Id}.jpg", updatedUser.AvatarUrl);
                
                // Act - Remove avatar completely
                await controller.RemoveAvatar(user.Id);
                
                // Assert Cleanup
                Assert.False(File.Exists(newPhysicalPath), "Avatar file should have been deleted by RemoveAvatar.");
                
                var removedUser = await finalDb.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
                Assert.True(string.IsNullOrEmpty(removedUser.AvatarUrl), "DB AvatarUrl should be empty after removal.");
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }
    }
}
