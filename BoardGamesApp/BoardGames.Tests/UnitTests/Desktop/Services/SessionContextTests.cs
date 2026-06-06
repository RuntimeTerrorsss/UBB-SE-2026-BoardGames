// <copyright file="SessionContextTests.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using NUnit.Framework;

namespace BoardGames.Tests.Services
{
    [TestFixture]
    public sealed class SessionContextTests
    {
        [Test]
        public void Populate_ValidProfile_CopiesFieldsAndMarksLoggedIn()
        {
            var systemUnderTest = new SessionContext();
            var accountId = Guid.NewGuid();
            var accountProfile = new AccountProfileDTO
            {
                Id = accountId,
                PamUserId = 7,
                Username = "player-one",
                DisplayName = "Player One",
                Email = "player@example.com",
                Role = new RoleDTO { Name = AppRoles.Administrator },
                AvatarUrl = "/avatars/player.png",
                IsSuspended = true,
                IsLocked = true,
                PhoneNumber = "0700000000",
                Country = "Romania",
                City = "Cluj-Napoca",
                StreetName = "Memorandumului",
                StreetNumber = "10",
            };

            systemUnderTest.Populate(accountProfile);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.IsLoggedIn, Is.True);
                Assert.That(systemUnderTest.AccountId, Is.EqualTo(accountId));
                Assert.That(systemUnderTest.PamUserId, Is.EqualTo(7));
                Assert.That(systemUnderTest.Username, Is.EqualTo("player-one"));
                Assert.That(systemUnderTest.DisplayName, Is.EqualTo("Player One"));
                Assert.That(systemUnderTest.Email, Is.EqualTo("player@example.com"));
                Assert.That(systemUnderTest.Role, Is.EqualTo(AppRoles.Administrator));
                Assert.That(systemUnderTest.AvatarUrl, Is.EqualTo("/avatars/player.png"));
                Assert.That(systemUnderTest.IsSuspended, Is.True);
                Assert.That(systemUnderTest.IsLocked, Is.True);
                Assert.That(systemUnderTest.PhoneNumber, Is.EqualTo("0700000000"));
                Assert.That(systemUnderTest.Country, Is.EqualTo("Romania"));
                Assert.That(systemUnderTest.City, Is.EqualTo("Cluj-Napoca"));
                Assert.That(systemUnderTest.StreetName, Is.EqualTo("Memorandumului"));
                Assert.That(systemUnderTest.StreetNumber, Is.EqualTo("10"));
            }
        }

        [Test]
        public void Populate_ProfileWithNullStrings_UsesEmptyStringsAndDefaultRole()
        {
            var systemUnderTest = new SessionContext();
            var accountProfile = new AccountProfileDTO
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
            };

            systemUnderTest.Populate(accountProfile);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.IsLoggedIn, Is.True);
                Assert.That(systemUnderTest.Username, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.DisplayName, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.Email, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.Role, Is.EqualTo(AppRoles.StandardUser));
                Assert.That(systemUnderTest.AvatarUrl, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.PhoneNumber, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.Country, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.City, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.StreetName, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.StreetNumber, Is.EqualTo(string.Empty));
            }
        }

        [Test]
        public void Populate_NullProfile_ThrowsArgumentNullException()
        {
            var systemUnderTest = new SessionContext();

            Assert.Throws<ArgumentNullException>(new Action(() => systemUnderTest.Populate(null!)));
        }

        [Test]
        public void Clear_PopulatedSession_ResetsAllFieldsAndMarksLoggedOut()
        {
            var systemUnderTest = new SessionContext();
            systemUnderTest.Populate(new AccountProfileDTO
            {
                Id = Guid.NewGuid(),
                PamUserId = 4,
                Username = "player-two",
                DisplayName = "Player Two",
                Email = "player2@example.com",
                Role = new RoleDTO { Name = AppRoles.Administrator },
                AvatarUrl = "/avatars/player2.png",
                IsSuspended = true,
                IsLocked = true,
                PhoneNumber = "0711111111",
                Country = "Romania",
                City = "Sibiu",
                StreetName = "Cetatii",
                StreetNumber = "3",
            });

            systemUnderTest.Clear();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(systemUnderTest.IsLoggedIn, Is.False);
                Assert.That(systemUnderTest.AccountId, Is.EqualTo(Guid.Empty));
                Assert.That(systemUnderTest.PamUserId, Is.Null);
                Assert.That(systemUnderTest.Username, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.DisplayName, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.Email, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.Role, Is.EqualTo(AppRoles.StandardUser));
                Assert.That(systemUnderTest.AvatarUrl, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.IsSuspended, Is.False);
                Assert.That(systemUnderTest.IsLocked, Is.False);
                Assert.That(systemUnderTest.PhoneNumber, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.Country, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.City, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.StreetName, Is.EqualTo(string.Empty));
                Assert.That(systemUnderTest.StreetNumber, Is.EqualTo(string.Empty));
            }
        }
    }
}
