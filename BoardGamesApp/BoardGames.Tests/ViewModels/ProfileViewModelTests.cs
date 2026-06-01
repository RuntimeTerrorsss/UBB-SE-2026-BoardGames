//using System;
//using System.Threading.Tasks;
//using System.Windows.Input;
//using BoardGames.Desktop.Commands;
//using BoardGames.Desktop.ViewModels;
//using BoardGames.Shared.DTO;
//using BoardGames.Shared.ProxyServices;
//using BoardGames.Tests.Fakes;
//using NUnit.Framework;

//namespace BoardGames.Tests.ViewModels
//{
//    [TestFixture]
//    public sealed class ProfileViewModelTests
//    {
//        private FakeClientAccountService accountService = null!;
//        private FakeClientAuthService authService = null!;
//        private FakeFilePickerService filePickerService = null!;
//        private FakeSessionContext sessionContext = null!;
//        private ProfileViewModel systemUnderTest = null!;
//        private Guid testAccountId;

//        [SetUp]
//        public void SetUp()
//        {
//            this.testAccountId = Guid.NewGuid();
//            this.accountService = new FakeClientAccountService();
//            this.authService = new FakeClientAuthService();
//            this.filePickerService = new FakeFilePickerService();
//            this.sessionContext = new FakeSessionContext
//            {
//                AccountId = this.testAccountId,
//                Username = "testuser",
//                DisplayName = "Test User"
//            };

//            this.systemUnderTest = new ProfileViewModel(
//                this.accountService,
//                this.authService,
//                this.filePickerService,
//                this.sessionContext);
//        }

//        [Test]
//        public async Task LoadProfileAsync_ValidData_PopulatesProperties()
//        {
//            var profileData = new AccountProfileDTO { Username = "loaded_user", DisplayName = "Loaded Name" };
//            this.accountService.ProfileResult = ServiceResult<AccountProfileDTO>.Ok(profileData);

//            await this.systemUnderTest.LoadProfileAsync();

//            Assert.That(this.systemUnderTest.Username, Is.EqualTo("loaded_user"));
//            Assert.That(this.systemUnderTest.DisplayName, Is.EqualTo("Loaded Name"));
//        }

//        [Test]
//        public void SaveProfileCommand_InvalidData_SetsValidationErrors()
//        {
//            this.systemUnderTest.DisplayName = "A";
//            this.accountService.UpdateProfileResult = ServiceResult.Fail("DisplayName|Display name must be at least 2 characters.");

//            this.systemUnderTest.SaveProfileCommand.Execute(null);

//            Assert.That(this.systemUnderTest.DisplayNameError, Is.EqualTo("Display name must be at least 2 characters."));
//        }

//        [Test]
//        public async Task SaveNewPasswordCommand_PasswordsDoNotMatch_SetsConfirmError()
//        {
//            this.systemUnderTest.NewPassword = "Pass";
//            this.systemUnderTest.ConfirmPassword = "NoMatch";

//            this.systemUnderTest.SaveNewPasswordCommand.Execute(null);
//            await Task.Delay(50);

//            Assert.That(this.systemUnderTest.ConfirmPasswordError, Is.EqualTo("Passwords do not match."));
//            Assert.That(this.accountService.ChangePasswordCallCount, Is.EqualTo(0));
//        }

//        [Test]
//        public async Task SignOutCommand_Executes_ClearsSessionAndTriggersCallback()
//        {
//            bool callbackTriggered = false;
//            this.systemUnderTest.OnSignOutSuccess = () => callbackTriggered = true;

//            this.systemUnderTest.SignOutCommand.Execute(null);
//            await Task.Delay(50);

//            Assert.That(this.sessionContext.IsLoggedIn, Is.False);
//            Assert.That(this.authService.LogoutCallCount, Is.EqualTo(1));
//            Assert.That(callbackTriggered, Is.True);
//        }

//        [Test]
//        public async Task SelectAvatarCommand_UserPicksFile_UpdatesAvatarUrl()
//        {
//            string fakePath = "C:\\avatar.jpg";
//            this.filePickerService.SelectedPath = fakePath;

//            this.systemUnderTest.SelectAvatarCommand.Execute(null);
//            await Task.Delay(50);

//            Assert.That(this.systemUnderTest.AvatarUrl, Is.EqualTo(fakePath));
//        }
//    }
//}