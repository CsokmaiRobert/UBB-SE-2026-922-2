using System;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.Utilities;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Utilities
{
    [TestFixture]
    public sealed class CurrentUserContextTests
    {
        [Test]
        public void CurrentUserId_ReturnsEmptyForFreshSessionAndReflectsAccountIdAfterPopulate()
        {
            var session = new SessionContext();
            var currentUserContext = new CurrentUserContext(session);

            Assert.That(currentUserContext.CurrentUserId, Is.EqualTo(Guid.Empty));

            var freshAccountId = Guid.NewGuid();
            session.Populate(new AccountProfileDataTransferObject { Id = freshAccountId });

            Assert.That(currentUserContext.CurrentUserId, Is.EqualTo(freshAccountId));
        }
    }
}
