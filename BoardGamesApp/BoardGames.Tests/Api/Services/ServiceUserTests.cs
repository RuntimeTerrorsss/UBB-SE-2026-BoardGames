using System;
using System.Collections.Generic;
using System.Linq;
using BoardGames.Tests.Fakes;
using NUnit.Framework;
using UserService = BoardGames.Api.Services.UserService;

namespace BoardGames.Tests.Api.Services
{
    [TestFixture]
    public sealed class ServiceUserTests
    {
        private readonly Guid currentAccountId = Guid.NewGuid();
        private readonly Guid secondAccountId = Guid.NewGuid();
        private readonly Guid thirdAccountId = Guid.NewGuid();

        private FakeAccountRepository repository = null!;
        private UserService service = null!;

        [SetUp]
        public void SetUp()
        {
            repository = new FakeAccountRepository();
            service = new UserService(repository, new UserMapper());
        }

        [Test]
        public void GetUsersExcept_WithMultipleAccounts_ReturnsAllAccountsBesidesTheCurrentOne()
        {
            var accounts = new List<Account>
            {
                new Account { Id = secondAccountId, DisplayName = "Maria" },
                new Account { Id = thirdAccountId, DisplayName = "Gabi" },
            };

            repository.Accounts = accounts;

            var result = service.GetUsersExcept(currentAccountId);

            Assert.That(result.Any(user => user.Id == secondAccountId && user.DisplayName == "Maria"), Is.True);
            Assert.That(result.Any(user => user.Id == thirdAccountId && user.DisplayName == "Gabi"), Is.True);
        }

        [Test]
        public void GetUsersExcept_WhenNoOtherAccountsExist_ReturnsEmptyList()
        {
            repository.Accounts = new List<Account>
            {
                new Account { Id = currentAccountId, DisplayName = "Me" },
            };

            var result = service.GetUsersExcept(currentAccountId);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetUsersExcept_WhenThereAreNoAccounts_ReturnsEmptyList()
        {
            repository.Accounts = new List<Account>();

            var result = service.GetUsersExcept(currentAccountId);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetUsersExcept_WithMultipleAccounts_ReturnsCorrectNumberOfAccounts()
        {
            var accounts = new List<Account>
            {
                new Account { Id = currentAccountId, DisplayName = "Me" },
                new Account { Id = secondAccountId, DisplayName = "Alice" },
                new Account { Id = thirdAccountId, DisplayName = "Bob" },
            };

            repository.Accounts = accounts;

            var result = service.GetUsersExcept(currentAccountId);

            Assert.That(result.Select(user => user.Id), Does.Not.Contain(currentAccountId));
            Assert.That(result, Has.Count.EqualTo(2));
        }
    }
}
