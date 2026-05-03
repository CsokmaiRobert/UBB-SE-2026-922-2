using System;
using System.Collections.Generic;
using System.Linq;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
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
            this.repository = new FakeAccountRepository();
            this.service = new UserService(this.repository, new UserMapper());
        }

        [Test]
        public void GetUsersExcept_WithMultipleAccounts_ReturnsAllAccountsBesidesTheCurrentOne()
        {
            var accounts = new List<Account>
            {
                new Account { Id = this.secondAccountId, DisplayName = "Maria" },
                new Account { Id = this.thirdAccountId, DisplayName = "Gabi" },
            };

            this.repository.Accounts = accounts;

            var result = this.service.GetUsersExcept(this.currentAccountId);

            Assert.That(result.Any(user => user.Id == this.secondAccountId && user.DisplayName == "Maria"), Is.True);
            Assert.That(result.Any(user => user.Id == this.thirdAccountId && user.DisplayName == "Gabi"), Is.True);
        }

        [Test]
        public void GetUsersExcept_WhenNoOtherAccountsExist_ReturnsEmptyList()
        {
            this.repository.Accounts = new List<Account>
            {
                new Account { Id = this.currentAccountId, DisplayName = "Me" },
            };

            var result = this.service.GetUsersExcept(this.currentAccountId);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetUsersExcept_WhenThereAreNoAccounts_ReturnsEmptyList()
        {
            this.repository.Accounts = new List<Account>();

            var result = this.service.GetUsersExcept(this.currentAccountId);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetUsersExcept_WithMultipleAccounts_ReturnsCorrectNumberOfAccounts()
        {
            var accounts = new List<Account>
            {
                new Account { Id = this.currentAccountId, DisplayName = "Me" },
                new Account { Id = this.secondAccountId, DisplayName = "Alice" },
                new Account { Id = this.thirdAccountId, DisplayName = "Bob" },
            };

            this.repository.Accounts = accounts;

            var result = this.service.GetUsersExcept(this.currentAccountId);

            Assert.That(result.Select(user => user.Id), Does.Not.Contain(this.currentAccountId));
            Assert.That(result, Has.Count.EqualTo(2));
        }
    }
}
