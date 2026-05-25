using System;
using BoardRentAndProperty.Api.Services;
using NUnit.Framework;

namespace BoardRentAndProperty.Tests.Api.Services
{
    [TestFixture]
    public sealed class DateRangeValidationHelperTests
    {
        [Test]
        public void HasValidFutureDateRange_StartDateAtOrAfterEndDate_ReturnsFalse()
        {
            DateTime laterStartDate = DateTime.UtcNow.Date.AddDays(5);
            DateTime earlierEndDate = DateTime.UtcNow.Date.AddDays(2);
            DateTime sameDate = DateTime.UtcNow.Date.AddDays(3);

            Assert.That(DateRangeValidationHelper.HasValidFutureDateRange(laterStartDate, earlierEndDate), Is.False);
            Assert.That(DateRangeValidationHelper.HasValidFutureDateRange(sameDate, sameDate), Is.False);
        }

        [Test]
        public void HasValidFutureDateRange_StartDateInPast_ReturnsFalse()
        {
            DateTime startDate = DateTime.UtcNow.Date.AddDays(-1);
            DateTime endDate = DateTime.UtcNow.Date.AddDays(3);

            bool isValid = DateRangeValidationHelper.HasValidFutureDateRange(startDate, endDate);

            Assert.That(isValid, Is.False);
        }

        [Test]
        public void HasValidFutureDateRange_OrderedFutureRange_ReturnsTrue()
        {
            DateTime startDate = DateTime.UtcNow.Date.AddDays(10);
            DateTime endDate = DateTime.UtcNow.Date.AddDays(15);

            bool isValid = DateRangeValidationHelper.HasValidFutureDateRange(startDate, endDate);

            Assert.That(isValid, Is.True);
        }
    }
}
