using System;

namespace BoardRentAndProperty.Services
{
    internal static class DateRangeValidationHelper
    {
        public static bool HasValidFutureDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate.Date >= endDate.Date)
            {
                return false;
            }

            return startDate.Date >= DateTime.UtcNow.Date;
        }
    }
}
