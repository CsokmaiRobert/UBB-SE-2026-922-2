using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

namespace BoardRentAndProperty.Services
{
    public class ToastNotificationService : IToastNotificationService
    {
        private const string NavigationKey = "navigate";
        private const string NotificationsPageKey = "NotificationsPage";

        public void Show(string notificationTitle, string notificationBody)
        {
            var toastXmlString = $@"
                <toast launch=""{NavigationKey}={NotificationsPageKey}"">
                    <visual>
                        <binding template=""ToastGeneric"">
                            <text>{notificationTitle}</text>
                            <text>{notificationBody}</text>
                        </binding>
                    </visual>
                </toast>";

            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(toastXmlString);

            var toast = new ToastNotification(xmlDocument);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}