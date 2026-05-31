using BookingBoardGames.Data.Interfaces;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BoardGames.Tests.UnitTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetUserByIdAsync_ValidId_ReturnsUser()
        {

            var expectedUser = new User();
            _userRepositoryMock.Setup(repo => repo.GetById(1)).ReturnsAsync(expectedUser);


            var result = await _userService.GetUserByIdAsync(1);


            Assert.Equal(expectedUser, result);
            _userRepositoryMock.Verify(repo => repo.GetById(1), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_WhenCalled_ReturnsUserList()
        {

            var expectedUsers = new List<User> { new User(), new User() };
            _userRepositoryMock.Setup(repo => repo.GetAll()).ReturnsAsync(expectedUsers);


            var result = await _userService.GetAllUsersAsync();


            Assert.Equal(expectedUsers, result);
            _userRepositoryMock.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_InvalidCredentials_ReturnsNull()
        {

            _userRepositoryMock.Setup(repo => repo.Login(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((User?)null);


            var result = await _userService.LoginAsync("testuser", "wrongpassword");


            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_UserIsSuspended_ReturnsNull()
        {

            var suspendedUser = new User { IsSuspended = true };
            _userRepositoryMock.Setup(repo => repo.Login("testuser", "password123"))
                .ReturnsAsync(suspendedUser);


            var result = await _userService.LoginAsync("testuser", "password123");


            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentialsAndNotSuspended_ReturnsUser()
        {

            var validUser = new User { IsSuspended = false };
            _userRepositoryMock.Setup(repo => repo.Login("testuser", "password123"))
                .ReturnsAsync(validUser);


            var result = await _userService.LoginAsync("testuser", "password123");


            Assert.Equal(validUser, result);
        }

        [Theory]
        [InlineData("", "Display", "Email", "Hash", "City", "Country")]
        [InlineData("User", "", "Email", "Hash", "City", "Country")]
        [InlineData("User", "Display", "", "Hash", "City", "Country")]
        [InlineData("User", "Display", "Email", "", "City", "Country")]
        [InlineData("User", "Display", "Email", "Hash", "", "Country")]
        [InlineData("User", "Display", "Email", "Hash", "City", "")]
        [InlineData(null, "Display", "Email", "Hash", "City", "Country")]
        public async Task RegisterUserAsync_MissingRequiredFields_ReturnsFalse(
            string username, string displayName, string email, string passwordHash, string city, string country)
        {

            var invalidUser = new User
            {
                Username = username,
                DisplayName = displayName,
                Email = email,
                PasswordHash = passwordHash,
                City = city,
                Country = country
            };


            var result = await _userService.RegisterUserAsync(invalidUser);


            Assert.False(result);
            _userRepositoryMock.Verify(repo => repo.Register(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUserAsync_ValidUser_ReturnsRepositoryResult()
        {

            var validUser = new User
            {
                Username = "User",
                DisplayName = "Display",
                Email = "Email@test.com",
                PasswordHash = "Hash",
                City = "City",
                Country = "Country"
            };

            _userRepositoryMock.Setup(repo => repo.Register(validUser)).ReturnsAsync(true);


            var result = await _userService.RegisterUserAsync(validUser);


            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.Register(validUser), Times.Once);
        }

        [Fact]
        public async Task GetBalanceAsync_ValidUserId_ReturnsBalance()
        {

            decimal expectedBalance = 150.50m;
            _userRepositoryMock.Setup(repo => repo.GetUserBalance(1)).ReturnsAsync(expectedBalance);


            var result = await _userService.GetBalanceAsync(1);


            Assert.Equal(expectedBalance, result);
            _userRepositoryMock.Verify(repo => repo.GetUserBalance(1), Times.Once);
        }

        [Fact]
        public async Task UpdateBalanceAsync_ValidInputs_CallsRepository()
        {

            int userId = 1;
            decimal amount = 50.00m;
            _userRepositoryMock.Setup(repo => repo.UpdateBalance(userId, amount)).Returns(Task.CompletedTask);


            await _userService.UpdateBalanceAsync(userId, amount);


            _userRepositoryMock.Verify(repo => repo.UpdateBalance(userId, amount), Times.Once);
        }
    }
}