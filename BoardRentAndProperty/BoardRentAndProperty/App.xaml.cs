using System;
using Microsoft.UI.Xaml;

namespace BoardRentAndProperty
{
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }

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
