// <copyright file="AuthProxyServiceAdapter.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;

namespace BoardGames.Web.Infrastructure
{
    public sealed class AuthProxyServiceAdapter : IAuthProxyService
    {
        private readonly IAuthService authService;

        public AuthProxyServiceAdapter(IAuthService authService)
        {
            this.authService = authService;
        }

        public async Task<AccountProfileDTO> LoginAsync(LoginDTO body, CancellationToken cancellationToken = default)
            => (await this.authService.LoginAsync(body, cancellationToken)).ThrowIfFailed();

        public async Task RegisterAsync(RegisterDTO body, CancellationToken cancellationToken = default)
            => (await this.authService.RegisterAsync(body, cancellationToken)).ThrowIfFailed();

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
            => (await this.authService.LogoutAsync(cancellationToken)).ThrowIfFailed();

        public async Task<string> ForgotPasswordAsync(CancellationToken cancellationToken = default)
            => (await this.authService.ForgotPasswordAsync(cancellationToken)).ThrowIfFailed();
    }
}
