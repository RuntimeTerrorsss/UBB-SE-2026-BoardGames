using System;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Api.Security;
using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using BoardGames.Shared.Common;

namespace BoardGames.Api.Services
{
    public class AuthService : IAuthService
    {
        private const string StandardUserRoleName = "Standard User";

        private readonly IAccountRepository accountRepository;
        private readonly IFailedLoginRepository failedLoginRepository;

        public AuthService(IAccountRepository accountRepository, IFailedLoginRepository failedLoginRepository)
        {
            this.accountRepository = accountRepository;
            this.failedLoginRepository = failedLoginRepository;
        }

        public async Task<ServiceResult<bool>> RegisterAsync(RegisterDTO registrationRequest)
        {

            var (isPasswordValid, passwordErrorMessage) = PasswordValidator.Validate(registrationRequest.Password);
            if (!isPasswordValid)
                return ServiceResult<bool>.Fail(passwordErrorMessage ?? "Password is invalid.");

            var existingByUsername = await accountRepository.GetByUsernameAsync(registrationRequest.Username);
            if (existingByUsername != null)
            {
                return ServiceResult<bool>.Fail("Username|Username is already taken.");
            }

            var existingByEmail = await accountRepository.GetByEmailAsync(registrationRequest.Email);
            if (existingByEmail != null)
            {
                return ServiceResult<bool>.Fail("Email|This email address is already registered.");
            }

            var newAccount = new Account
            {
                Id = Guid.NewGuid(),
                DisplayName = registrationRequest.DisplayName,
                Username = registrationRequest.Username,
                Email = registrationRequest.Email,
                PasswordHash = PasswordHasher.HashPassword(registrationRequest.Password),
                PhoneNumber = registrationRequest.PhoneNumber ?? string.Empty,
                AvatarUrl = string.Empty,
                Country = registrationRequest.Country ?? string.Empty,
                City = registrationRequest.City ?? string.Empty,
                StreetName = registrationRequest.StreetName ?? string.Empty,
                StreetNumber = registrationRequest.StreetNumber ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsSuspended = false,
            };

            await accountRepository.AddAsync(newAccount);
            await accountRepository.AddRoleAsync(newAccount.Id, StandardUserRoleName);

            return ServiceResult<bool>.Ok(true);
        }

        public async Task<ServiceResult<AccountProfileDTO>> LoginAsync(LoginDTO loginRequest)
        {
            var account = await accountRepository.GetByUsernameAsync(loginRequest.UsernameOrEmail)
                       ?? await accountRepository.GetByEmailAsync(loginRequest.UsernameOrEmail);
            if (account == null)
            {
                return ServiceResult<AccountProfileDTO>.Fail("Invalid username or password.");
            }

            if (account.IsSuspended)
            {
                return ServiceResult<AccountProfileDTO>.Fail("This account has been suspended.");
            }

            var failedLoginAttempt = await failedLoginRepository.GetByAccountIdAsync(account.Id);
            if (failedLoginAttempt?.LockedUntil.HasValue == true
                && failedLoginAttempt.LockedUntil.Value > DateTime.UtcNow)
            {
                return ServiceResult<AccountProfileDTO>.Fail("This account is locked. Please contact an administrator or try again later.");
            }

            if (!PasswordHasher.VerifyPassword(loginRequest.Password, account.PasswordHash))
            {
                await failedLoginRepository.IncrementAsync(account.Id);
                return ServiceResult<AccountProfileDTO>.Fail("Invalid username or password.");
            }

            await failedLoginRepository.ResetAsync(account.Id);
            string primaryRole = account.Roles?.FirstOrDefault()?.Name ?? StandardUserRoleName;
            bool isLocked = failedLoginAttempt?.LockedUntil.HasValue == true && failedLoginAttempt.LockedUntil.Value > DateTime.UtcNow;

            return ServiceResult<AccountProfileDTO>.Ok(new AccountProfileDTO
            {
                Id = account.Id,
                PamUserId = account.PamUserId,
                Username = account.Username,
                DisplayName = account.DisplayName,
                Email = account.Email,
                PhoneNumber = account.PhoneNumber,
                AvatarUrl = account.AvatarUrl,
                Country = account.Country,
                City = account.City,
                StreetName = account.StreetName,
                StreetNumber = account.StreetNumber,
                IsSuspended = account.IsSuspended,
                IsLocked = isLocked,
                Role = new RoleDTO { Name = primaryRole },
            });
        }

        public Task<ServiceResult<bool>> LogoutAsync() =>
            Task.FromResult(ServiceResult<bool>.Ok(true));

        public Task<ServiceResult<string>> ForgotPasswordAsync() =>
            Task.FromResult(ServiceResult<string>.Ok("Please contact the Administrator at admin@boardrent.com."));
    }
}
