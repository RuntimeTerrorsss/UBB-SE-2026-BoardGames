//// <copyright file="UserServiceTests.cs" company="BoardRent">
//// Copyright (c) BoardRent. All rights reserved.
//// </copyright>

//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace BoardGames.Tests.UnitTests
//{
//    public class UserServiceTests
//    {
//        private readonly Mock<IUserRepository> _userRepositoryMock;
//        private readonly UserService _userService;

//        public UserServiceTests()
//        {
//            this._userRepositoryMock = new Mock<IUserRepository>();
//            this._userService = new UserService(this._userRepositoryMock.Object);
//        }

//        [Fact]
//        public async Task GetUserByIdAsync_ValidId_ReturnsUser()
//        {
//            var expectedUser = new User();
//            this._userRepositoryMock.Setup(repo => repo.GetById(1)).ReturnsAsync(expectedUser);

//            var result = await this._userService.GetUserByIdAsync(1);

//            Assert.Equal(expectedUser, result);
//            this._userRepositoryMock.Verify(repo => repo.GetById(1), Times.Once);
//        }

//        [Fact]
//        public async Task GetAllUsersAsync_WhenCalled_ReturnsUserList()
//        {
//            var expectedUsers = new List<User> { new User(), new User() };
//            this._userRepositoryMock.Setup(repo => repo.GetAll()).ReturnsAsync(expectedUsers);

//            var result = await this._userService.GetAllUsersAsync();

//            Assert.Equal(expectedUsers, result);
//            this._userRepositoryMock.Verify(repo => repo.GetAll(), Times.Once);
//        }

//        [Fact]
//        public async Task LoginAsync_InvalidCredentials_ReturnsNull()
//        {
//            this._userRepositoryMock.Setup(repo => repo.Login(It.IsAny<string>(), It.IsAny<string>()))
//                .ReturnsAsync((User?)null);

//            var result = await this._userService.LoginAsync("testuser", "wrongpassword");

//            Assert.Null(result);
//        }

//        [Fact]
//        public async Task LoginAsync_UserIsSuspended_ReturnsNull()
//        {
//            var suspendedUser = new User { IsSuspended = true };
//            this._userRepositoryMock.Setup(repo => repo.Login("testuser", "password123"))
//                .ReturnsAsync(suspendedUser);

//            var result = await this._userService.LoginAsync("testuser", "password123");

//            Assert.Null(result);
//        }

//        [Fact]
//        public async Task LoginAsync_ValidCredentialsAndNotSuspended_ReturnsUser()
//        {
//            var validUser = new User { IsSuspended = false };
//            this._userRepositoryMock.Setup(repo => repo.Login("testuser", "password123"))
//                .ReturnsAsync(validUser);

//            var result = await this._userService.LoginAsync("testuser", "password123");

//            Assert.Equal(validUser, result);
//        }

//        [Theory]
//        [InlineData("", "Display", "Email", "Hash", "City", "Country")]
//        [InlineData("User", "", "Email", "Hash", "City", "Country")]
//        [InlineData("User", "Display", "", "Hash", "City", "Country")]
//        [InlineData("User", "Display", "Email", "", "City", "Country")]
//        [InlineData("User", "Display", "Email", "Hash", "", "Country")]
//        [InlineData("User", "Display", "Email", "Hash", "City", "")]
//        [InlineData(null, "Display", "Email", "Hash", "City", "Country")]
//        public async Task RegisterUserAsync_MissingRequiredFields_ReturnsFalse(
//            string username, string displayName, string email, string passwordHash, string city, string country)
//        {
//            var invalidUser = new User
//            {
//                Username = username,
//                DisplayName = displayName,
//                Email = email,
//                PasswordHash = passwordHash,
//                City = city,
//                Country = country,
//            };

//            var result = await this._userService.RegisterUserAsync(invalidUser);

//            Assert.False(result);
//            this._userRepositoryMock.Verify(repo => repo.Register(It.IsAny<User>()), Times.Never);
//        }

//        [Fact]
//        public async Task RegisterUserAsync_ValidUser_ReturnsRepositoryResult()
//        {
//            var validUser = new User
//            {
//                Username = "User",
//                DisplayName = "Display",
//                Email = "Email@test.com",
//                PasswordHash = "Hash",
//                City = "City",
//                Country = "Country",
//            };

//            this._userRepositoryMock.Setup(repo => repo.Register(validUser)).ReturnsAsync(true);

//            var result = await this._userService.RegisterUserAsync(validUser);

//            Assert.True(result);
//            this._userRepositoryMock.Verify(repo => repo.Register(validUser), Times.Once);
//        }

//        [Fact]
//        public async Task GetBalanceAsync_ValidUserId_ReturnsBalance()
//        {
//            decimal expectedBalance = 150.50m;
//            this._userRepositoryMock.Setup(repo => repo.GetUserBalance(1)).ReturnsAsync(expectedBalance);

//            var result = await this._userService.GetBalanceAsync(1);

//            Assert.Equal(expectedBalance, result);
//            this._userRepositoryMock.Verify(repo => repo.GetUserBalance(1), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateBalanceAsync_ValidInputs_CallsRepository()
//        {
//            int userId = 1;
//            decimal amount = 50.00m;
//            this._userRepositoryMock.Setup(repo => repo.UpdateBalance(userId, amount)).Returns(Task.CompletedTask);

//            await this._userService.UpdateBalanceAsync(userId, amount);

//            this._userRepositoryMock.Verify(repo => repo.UpdateBalance(userId, amount), Times.Once);
//        }
//    }
//}
