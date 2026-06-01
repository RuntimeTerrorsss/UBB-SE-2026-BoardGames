// <copyright file="ClaimsPrincipalExtensionsTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;
using BoardGames.Web.Helpers;
using Xunit;

namespace BoardGames.Tests.Web
{
    public class ClaimsPrincipalExtensionsTests
    {
        [Fact]
        public void GetAccountId_ValidClaim_ReturnsGuid()
        {
            var accountId = Guid.NewGuid();
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
            });
            var principal = new ClaimsPrincipal(identity);

            Guid result = principal.GetAccountId();

            Assert.Equal(accountId, result);
        }

        [Fact]
        public void TryGetPamUserId_WithClaim_ReturnsTrue()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("PamUserId", "42"),
            });
            var principal = new ClaimsPrincipal(identity);

            bool found = principal.TryGetPamUserId(out int pamUserId);

            Assert.True(found);
            Assert.Equal(42, pamUserId);
        }

        [Fact]
        public void IsAdministrator_AdminRole_ReturnsTrue()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Role, AppRoles.Administrator),
            });
            var principal = new ClaimsPrincipal(identity);

            Assert.True(principal.IsAdministrator());
        }
    }
}
