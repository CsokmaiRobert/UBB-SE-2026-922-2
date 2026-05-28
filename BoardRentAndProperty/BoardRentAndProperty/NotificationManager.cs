using System;
using System.Collections.Generic;
#if WINDOWS
using Microsoft.Toolkit.Uwp.Notifications;
#endif

namespace BoardRentAndProperty
{
    internal class NotificationManager
    {
        public event EventHandler<IDictionary<string, string>>? NotificationClicked;

        public void Init()
        {
#if WINDOWS
            ToastNotificationManagerCompat.OnActivated += OnToastActivated;
#endif
        }

        public void Unregister()
        {
#if WINDOWS
            ToastNotificationManagerCompat.OnActivated -= OnToastActivated;
#endif
        }

#if WINDOWS
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
#endif

        public void ProcessLaunchActivationArgs(IDictionary<string, string> arguments)
        {
            NotificationClicked?.Invoke(this, arguments);
        }
    }
}