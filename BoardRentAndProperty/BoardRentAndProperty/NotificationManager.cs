using System;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.Notifications;

namespace BoardRentAndProperty
{
    internal class NotificationManager
    {
        public event EventHandler<IDictionary<string, string>>? NotificationClicked;

        public void Init()
        {
            ToastNotificationManagerCompat.OnActivated += OnToastActivated;
        }

        public void Unregister()
        {
            ToastNotificationManagerCompat.OnActivated -= OnToastActivated;
        }

        private void OnToastActivated(ToastNotificationActivatedEventArgsCompat args)
        {
            var toastArgs = ToastArguments.Parse(args.Argument);
            var arguments = new Dictionary<string, string>();
            foreach (var arg in toastArgs)
            {
                arguments[arg.Key] = arg.Value ?? string.Empty;
            }

            NotificationClicked?.Invoke(this, arguments);
        }

        public void ProcessLaunchActivationArgs(IDictionary<string, string> arguments)
        {
            NotificationClicked?.Invoke(this, arguments);
        }
    }
}
