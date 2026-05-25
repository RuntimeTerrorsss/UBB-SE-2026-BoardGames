using System;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardGames.Shared.DTO;
using BoardGames.Desktop.Services;
using BoardRentAndProperty.ViewModels;
using CommunityToolkit.Mvvm.Input;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class ProfileViewModelTests
    {
        private FakeClientAccountService accountService = null!;
        private FakeClientAuthService authService = null!;
        private FakeFilePickerService filePickerService = null!;
        private FakeSessionContext sessionContext = null!;
        private ProfileViewModel systemUnderTest = null!;
        private Guid testAccountId;

        [SetUp]
        public void SetUp()
        {
            testAccountId = Guid.NewGuid();
            accountService = new FakeClientAccountService();
            authService = new FakeClientAuthService();
            filePickerService = new FakeFilePickerService();
            sessionContext = new FakeSessionContext
            {
                AccountId = testAccountId,
                Username = "testuser",
                DisplayName = "Test User",
                Email = "test@test.com",
                PhoneNumber = string.Empty,
                Country = string.Empty,
                City = string.Empty,
                StreetName = string.Empty,
                StreetNumber = string.Empty,
            };

            systemUnderTest = new ProfileViewModel(
                accountService,
                authService,
                filePickerService,
                sessionContext);
        }

        [Test]
        public async Task LoadProfileAsync_ValidData_PopulatesProperties()
        {
            var profileData = new AccountProfileDataTransferObject
            {
                Id = testAccountId,
                Username = "loaded_user",
                DisplayName = "Loaded Name",
                Email = "loaded@test.com",
            };

            accountService.ProfileResult =
                ServiceResult<AccountProfileDataTransferObject>.Ok(profileData);

            await systemUnderTest.LoadProfileAsync();

            Assert.That(systemUnderTest.Username, Is.EqualTo("loaded_user"));
            Assert.That(systemUnderTest.DisplayName, Is.EqualTo("Loaded Name"));
            Assert.That(systemUnderTest.Email, Is.EqualTo("loaded@test.com"));
        }

        [Test]
        public async Task SaveProfileCommand_InvalidData_SetsDisplayNameError()
        {
            systemUnderTest.DisplayName = "A";

            var failureResult = ServiceResult<bool>.Fail("DisplayName|Display name must be between 2 and 50 characters long.");
            accountService.UpdateProfileResult = failureResult;

            await ((IAsyncRelayCommand)systemUnderTest.SaveProfileCommand).ExecuteAsync(null);

            Assert.That(systemUnderTest.DisplayNameError, Is.EqualTo("Display name must be between 2 and 50 characters long."));
        }

        [Test]
        public async Task SelectAvatarCommand_UserPicksFile_SetsAvatarUrlPreview()
        {
            string fakePath = "C:\\test_avatar.jpg";
            filePickerService.SelectedPath = fakePath;

            await ((IAsyncRelayCommand)systemUnderTest.SelectAvatarCommand).ExecuteAsync(null);

            Assert.That(systemUnderTest.AvatarUrl, Is.EqualTo(fakePath));
            Assert.That(accountService.UploadAvatarCallCount, Is.EqualTo(0));
        }

        [Test]
        public async Task SaveNewPasswordCommand_PasswordsDoNotMatch_SetsConfirmError()
        {
            systemUnderTest.NewPassword = "Password123!";
            systemUnderTest.ConfirmPassword = "DifferentPassword123!";

            await ((IAsyncRelayCommand)systemUnderTest.SaveNewPasswordCommand).ExecuteAsync(null);

            Assert.That(systemUnderTest.ConfirmPasswordError, Is.EqualTo("Passwords do not match."));
            Assert.That(accountService.ChangePasswordCallCount, Is.EqualTo(0));
        }
    }
}
