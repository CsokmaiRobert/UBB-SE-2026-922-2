using System.Collections.Generic;

namespace BoardRentAndProperty.Services
{
    public interface IDismissedNotificationStore
    {
        HashSet<int> Load(int currentUserId);

        void Save(int currentUserId, IEnumerable<int> dismissedNotificationIdentifiers);
    }
}