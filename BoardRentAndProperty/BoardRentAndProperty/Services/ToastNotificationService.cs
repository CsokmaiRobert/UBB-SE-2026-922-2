using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using BoardRentAndProperty.Services;

namespace BoardRentAndProperty.Services
{
    public class ToastNotificationService : IToastNotificationService
    {
        private const string NavigationKey = "navigate";
        private const string NotificationsPageKey = "NotificationsPage";

        public void Show(string notificationTitle, string notificationBody)
        {
            var notification = new AppNotificationBuilder()
                .AddArgument(NavigationKey, NotificationsPageKey)
                .AddText(notificationTitle)
                .AddText(notificationBody)
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }
    }
}