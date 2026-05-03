using Microsoft.Toolkit.Uwp.Notifications;

namespace BoardRentAndProperty.Services
{
    public class ToastNotificationService : IToastNotificationService
    {
        private const string NavigationKey = "navigate";
        private const string NotificationsPageKey = "NotificationsPage";

        public void Show(string notificationTitle, string notificationBody)
        {
            new ToastContentBuilder()
                .AddArgument(NavigationKey, NotificationsPageKey)
                .AddText(notificationTitle)
                .AddText(notificationBody)
                .Show();
        }
    }
}
