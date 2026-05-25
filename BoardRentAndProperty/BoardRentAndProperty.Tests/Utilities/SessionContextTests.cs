using System;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Utilities;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Utilities
{
    [TestFixture]
    public sealed class SessionContextTests
    {
        private SessionContext sessionContext = null!;

        [SetUp]
        public void SetUp()
        {
            this.sessionContext = new SessionContext();
        }

        [Test]
        public void NewSessionContext_StartsLoggedOutWithEmptyValues()
        {
            Assert.That(this.sessionContext.IsLoggedIn, Is.False);
            Assert.That(this.sessionContext.AccountId, Is.EqualTo(Guid.Empty));
            Assert.That(this.sessionContext.Username, Is.Empty);
            Assert.That(this.sessionContext.Role, Is.Empty);
        }

        [Test]
        public void Populate_WithCompleteProfile_CopiesAllFieldsAndMarksLoggedIn()
        {
            var accountIdentifier = Guid.NewGuid();
            var profile = new AccountProfileDataTransferObject
            {
                Id = accountIdentifier,
                Username = "alice42",
                DisplayName = "Alice",
                Email = "alice@test.com",
                PhoneNumber = "0712345678",
                Country = "Romania",
                City = "Cluj",
                StreetName = "Strada Mare",
                StreetNumber = "10A",
                Role = new RoleDataTransferObject { Id = Guid.NewGuid(), Name = "Administrator" },
            };

            this.sessionContext.Populate(profile);

            Assert.That(this.sessionContext.AccountId, Is.EqualTo(accountIdentifier));
            Assert.That(this.sessionContext.Username, Is.EqualTo("alice42"));
            Assert.That(this.sessionContext.Email, Is.EqualTo("alice@test.com"));
            Assert.That(this.sessionContext.Role, Is.EqualTo("Administrator"));
            Assert.That(this.sessionContext.IsLoggedIn, Is.True);
        }

        [Test]
        public void Populate_WithMissingOptionalsAndNullRole_FallsBackToEmptyStringsAndStandardUser()
        {
            var profile = new AccountProfileDataTransferObject
            {
                Id = Guid.NewGuid(),
                Username = null!,
                Role = null!,
            };

            this.sessionContext.Populate(profile);

            Assert.That(this.sessionContext.Username, Is.Empty);
            Assert.That(this.sessionContext.Role, Is.EqualTo(AppRoles.StandardUser));
        }

        [Test]
        public void Populate_WithNullProfile_DoesNotModifyExistingState()
        {
            this.sessionContext.Populate(new AccountProfileDataTransferObject
            {
                Id = Guid.NewGuid(),
                Username = "first",
                Role = new RoleDataTransferObject { Id = Guid.NewGuid(), Name = "Standard User" },
            });

            this.sessionContext.Populate(null!);

            Assert.That(this.sessionContext.Username, Is.EqualTo("first"));
            Assert.That(this.sessionContext.IsLoggedIn, Is.True);
        }

        [Test]
        public void Clear_AfterPopulate_ResetsAllFieldsAndMarksLoggedOut()
        {
            this.sessionContext.Populate(new AccountProfileDataTransferObject
            {
                Id = Guid.NewGuid(),
                Username = "bob",
                Role = new RoleDataTransferObject { Id = Guid.NewGuid(), Name = "Administrator" },
            });

            this.sessionContext.Clear();

            Assert.That(this.sessionContext.IsLoggedIn, Is.False);
            Assert.That(this.sessionContext.Username, Is.Empty);
            Assert.That(this.sessionContext.Role, Is.Empty);
        }
    }
}
