using System;
using System.Collections.Generic;
using System.Linq;
using BoardRentAndProperty.Api.Mappers;
using BoardRentAndProperty.Api.Models;
using BoardRentAndProperty.Api.Services;
using BoardRentAndProperty.Tests.Fakes;
using NUnit.Framework;
using UserService = BoardRentAndProperty.Api.Services.UserService;

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
        public void GetUsersExcept_ReturnsOtherAccountsAndOmitsCurrent()
        {
            this.repository.Accounts = new List<Account>
            {
                new Account { Id = this.currentAccountId, DisplayName = "Me" },
                new Account { Id = this.secondAccountId, DisplayName = "Maria" },
                new Account { Id = this.thirdAccountId, DisplayName = "Gabi" },
            };

            var result = this.service.GetUsersExcept(this.currentAccountId);

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Select(user => user.Id), Does.Not.Contain(this.currentAccountId));
            Assert.That(result.Any(user => user.DisplayName == "Maria"), Is.True);
        }

        [Test]
        public void GetUsersExcept_WhenOnlyCurrentAccountOrNoAccounts_ReturnsEmptyList()
        {
            this.repository.Accounts = new List<Account>
            {
                new Account { Id = this.currentAccountId, DisplayName = "Me" },
            };
            Assert.That(this.service.GetUsersExcept(this.currentAccountId), Is.Empty);

            this.repository.Accounts = new List<Account>();
            Assert.That(this.service.GetUsersExcept(this.currentAccountId), Is.Empty);
        }
    }
}
