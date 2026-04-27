using System;
using Microsoft.UI.Xaml;

namespace BoardRentAndProperty
{
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }

        // TODO(task-9): Stubbed for build to succeed. Populated by the merged DI graph in App.xaml.cs Task 9.
        // Until Task 9 lands, any access to App.Services will throw NullReferenceException.
        public static IServiceProvider Services { get; private set; } = default!;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}
