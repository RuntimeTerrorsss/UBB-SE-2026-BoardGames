// <copyright file="SessionContextTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System;
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using NUnit.Framework;

namespace BoardGames.Tests.Services
{
    [TestFixture]
    public sealed class SessionContextTests
    {
        [Test]
        public void Populate_WithCompleteProfile_CopiesAllFields()
        {
            var sessionContext = new SessionContext();
            var accountProfile = BuildAccountProfile();

            sessionContext.Populate(accountProfile);

            Assert.That(sessionContext.IsLoggedIn, Is.True);
            Assert.That(sessionContext.AccountId, Is.EqualTo(accountProfile.Id));
            Assert.That(sessionContext.PamUserId, Is.EqualTo(accountProfile.PamUserId));
            Assert.That(sessionContext.Username, Is.EqualTo(accountProfile.Username));
            Assert.That(sessionContext.DisplayName, Is.EqualTo(accountProfile.DisplayName));
            Assert.That(sessionContext.Email, Is.EqualTo(accountProfile.Email));
            Assert.That(sessionContext.Role, Is.EqualTo(accountProfile.Role!.Name));
            Assert.That(sessionContext.AvatarUrl, Is.EqualTo(accountProfile.AvatarUrl));
            Assert.That(sessionContext.IsSuspended, Is.EqualTo(accountProfile.IsSuspended));
            Assert.That(sessionContext.IsLocked, Is.EqualTo(accountProfile.IsLocked));
            Assert.That(sessionContext.PhoneNumber, Is.EqualTo(accountProfile.PhoneNumber));
            Assert.That(sessionContext.Country, Is.EqualTo(accountProfile.Country));
            Assert.That(sessionContext.City, Is.EqualTo(accountProfile.City));
            Assert.That(sessionContext.StreetName, Is.EqualTo(accountProfile.StreetName));
            Assert.That(sessionContext.StreetNumber, Is.EqualTo(accountProfile.StreetNumber));
        }

        [Test]
        public void Populate_WithMissingOptionalFields_UsesSafeDefaults()
        {
            var sessionContext = new SessionContext();

            sessionContext.Populate(new AccountProfileDTO
            {
                Id = Guid.NewGuid(),
                Username = null!,
                DisplayName = null!,
                Email = null!,
                Role = null,
                AvatarUrl = null!,
                PhoneNumber = null!,
                Country = null!,
                City = null!,
                StreetName = null!,
                StreetNumber = null!,
            });

            Assert.That(sessionContext.Username, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.DisplayName, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.Email, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.Role, Is.EqualTo(AppRoles.StandardUser));
            Assert.That(sessionContext.AvatarUrl, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.PhoneNumber, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.Country, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.City, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.StreetName, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.StreetNumber, Is.EqualTo(string.Empty));
        }

        [Test]
        public void IsLoggedIn_BeforeAndAfterPopulate_ReflectsAccountState()
        {
            var sessionContext = new SessionContext();

            Assert.That(sessionContext.IsLoggedIn, Is.False);

            sessionContext.Populate(new AccountProfileDTO { Id = Guid.NewGuid() });

            Assert.That(sessionContext.IsLoggedIn, Is.True);
        }

        [Test]
        public void Clear_AfterPopulate_ResetsAllFields()
        {
            var sessionContext = new SessionContext();
            sessionContext.Populate(BuildAccountProfile());

            sessionContext.Clear();

            Assert.That(sessionContext.IsLoggedIn, Is.False);
            Assert.That(sessionContext.AccountId, Is.EqualTo(Guid.Empty));
            Assert.That(sessionContext.PamUserId, Is.Null);
            Assert.That(sessionContext.Username, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.DisplayName, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.Email, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.Role, Is.EqualTo(AppRoles.StandardUser));
            Assert.That(sessionContext.AvatarUrl, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.IsSuspended, Is.False);
            Assert.That(sessionContext.IsLocked, Is.False);
            Assert.That(sessionContext.PhoneNumber, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.Country, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.City, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.StreetName, Is.EqualTo(string.Empty));
            Assert.That(sessionContext.StreetNumber, Is.EqualTo(string.Empty));
        }

        private static AccountProfileDTO BuildAccountProfile()
        {
            return new AccountProfileDTO
            {
                Id = Guid.NewGuid(),
                PamUserId = 41,
                Username = "alice",
                DisplayName = "Alice Example",
                Email = "alice@example.com",
                Role = new RoleDTO { Name = AppRoles.Administrator },
                AvatarUrl = "/images/alice.png",
                IsSuspended = true,
                IsLocked = true,
                PhoneNumber = "0712345678",
                Country = "Romania",
                City = "Cluj-Napoca",
                StreetName = "Memorandumului",
                StreetNumber = "12A",
            };
        }
    }
}
