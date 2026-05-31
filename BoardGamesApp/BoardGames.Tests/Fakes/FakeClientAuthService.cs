// <copyright file="FakeClientAuthService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Tests.Fakes
{
    internal sealed class FakeClientAuthService : IAuthService
    {
        public ServiceResult RegisterResult { get; set; } = ServiceResult.Ok();

        public ServiceResult<AccountProfileDTO> LoginResult { get; set; }
            = ServiceResult<AccountProfileDTO>.Ok(new AccountProfileDTO());

        public ServiceResult LogoutResult { get; set; } = ServiceResult.Ok();

        public ServiceResult<string> ForgotPasswordResult { get; set; }
            = ServiceResult<string>.Ok(string.Empty);

        public int RegisterCallCount { get; private set; }

        public int LoginCallCount { get; private set; }

        public RegisterDTO? LastRegisterRequest { get; private set; }

        public LoginDTO? LastLoginRequest { get; private set; }

        public Task<ServiceResult> RegisterAsync(RegisterDTO request, CancellationToken cancellationToken = default)
        {
            this.RegisterCallCount++;
            this.LastRegisterRequest = request;
            return Task.FromResult(this.RegisterResult);
        }

        public Task<ServiceResult<AccountProfileDTO>> LoginAsync(LoginDTO request, CancellationToken cancellationToken = default)
        {
            this.LoginCallCount++;
            this.LastLoginRequest = request;
            return Task.FromResult(this.LoginResult);
        }

        public Task<ServiceResult> LogoutAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.LogoutResult);
        }

        public Task<ServiceResult<string>> ForgotPasswordAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(this.ForgotPasswordResult);
        }
    }
}
