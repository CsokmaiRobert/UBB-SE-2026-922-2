using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BoardRentAndProperty.Data;
using BoardRentAndProperty.DataTransferObjects;
using BoardRentAndProperty.Mappers;
using BoardRentAndProperty.Models;
using BoardRentAndProperty.Repositories;
using BoardRentAndProperty.Services;
using BoardRentAndProperty.Services.Listeners;
using BoardRentAndProperty.Utilities;
using BoardRentAndProperty.ViewModels;
using BoardRentAndProperty.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using H.NotifyIcon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;

namespace BoardRentAndProperty
{
    public partial class App : Application
    {
        private const int DefaultProcessSlot = 1;
        private const int ProcessSlotArgumentIndex = 1;
        private const int KeyPartIndex = 0;
        private const int ValuePartIndex = 1;
        private const int SplitKeyValuePartsCount = 2;
        private const int DevModePrimaryProcessSlot = 1;
        private const int DevModeSecondaryProcessSlot = 2;
        private const int NoRunningProcessCount = 0;
        private const int SuccessExitCode = 0;

        private const string TwoWindowsEnvironmentKey = "TWO_WINDOWS";
        private const string EnabledEnvironmentValue = "true";
        private const string NotificationNavigationArgumentKey = "navigate";
        private const string TrayIconIdentityPrefix = "BoardRentAndProperty.TrayIcon";

        public static IServiceProvider Services { get; private set; } = default!;
        public static Window? MainWindow { get; set; }
        public Frame? RootFrame { get; set; }

        public string AppUserModelId { get; }
        public int CurrentProcessSlot { get; }
        public NotificationsViewModel? NotificationsViewModel { get; private set; }

        private TaskbarIcon? trayIcon;
        private static Process? notificationServerProcess;
        private static Process? secondClientProcess;

        private Window? mainWindow;
        private readonly bool shouldLaunchSecondClient;
        private INotificationRepository? notificationRepository;
        private INotificationService? notificationService;
        private IGameRepository? gameRepository;
        private IGameService? gameService;
        private readonly NotificationManager notificationManager;

        public App()
        {
            CurrentProcessSlot = GetProcessSlotFromArgs();
            shouldLaunchSecondClient = CurrentProcessSlot == DevModePrimaryProcessSlot && IsTwoWindowsEnabled();

            if (shouldLaunchSecondClient)
            {
                StartNotificationServer();
            }

            AppUserModelId = $"BoardRentAndProperty -- slot-{CurrentProcessSlot}";

            notificationManager = new NotificationManager();
            SetupNotificationManager();
            EnsureSingleInstance(AppUserModelId);

            ConfigureServices();

            Services.GetRequiredService<AppDbContext>().Database.EnsureCreated();

            InitializeServices();

            InitializeComponent();
        }

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            // mappers
            serviceCollection.AddSingleton<IMapper<Account, UserDTO, Guid>, UserMapper>();
            serviceCollection.AddSingleton<IMapper<Game, GameDTO, int>, GameMapper>();
            serviceCollection.AddSingleton<IMapper<Notification, NotificationDTO, int>, NotificationMapper>();
            serviceCollection.AddSingleton<IMapper<Rental, RentalDTO, int>, RentalMapper>();
            serviceCollection.AddSingleton<IMapper<Request, RequestDTO, int>, RequestMapper>();

            // PaM cross-cutting
            serviceCollection.AddSingleton<ICurrentUserContext, CurrentUserContext>();
            serviceCollection.AddSingleton<IToastNotificationService, ToastNotificationService>();
            serviceCollection.AddSingleton<IServerClient, NotificationClient>();

            // repositories
            serviceCollection.AddSingleton<IGameRepository, GameRepository>();
            serviceCollection.AddSingleton<IRequestRepository, RequestRepository>();
            serviceCollection.AddSingleton<IRentalRepository, RentalRepository>();
            serviceCollection.AddSingleton<INotificationRepository, NotificationRepository>();

            // PaM services
            serviceCollection.AddSingleton<IUserService, UserService>();
            serviceCollection.AddSingleton<IGameService, GameService>();
            serviceCollection.AddSingleton<IRentalService, RentalService>();
            serviceCollection.AddSingleton<INotificationService, NotificationService>();
            serviceCollection.AddSingleton<IRequestService, RequestService>();

            // PaM view models
            serviceCollection.AddSingleton<NotificationsViewModel>();
            serviceCollection.AddSingleton<MenuBarViewModel>();
            serviceCollection.AddTransient(serviceProvider => new ListingsViewModel(
                serviceProvider.GetRequiredService<IGameService>(),
                serviceProvider.GetRequiredService<ICurrentUserContext>().CurrentUserId));
            serviceCollection.AddTransient<CreateGameViewModel>();
            serviceCollection.AddTransient<EditGameViewModel>();
            serviceCollection.AddTransient<CreateRequestViewModel>();
            serviceCollection.AddTransient<CreateRentalViewModel>();
            serviceCollection.AddTransient<RequestsFromOthersViewModel>();
            serviceCollection.AddTransient<RequestsToOthersViewModel>();
            serviceCollection.AddTransient<RentalsFromOthersViewModel>();
            serviceCollection.AddTransient<RentalsToOthersViewModel>();

            // data layer
            serviceCollection.AddSingleton<AppDbContext>();

            // account domain repositories
            serviceCollection.AddSingleton<IAccountRepository, AccountRepository>();
            serviceCollection.AddSingleton<IFailedLoginRepository, FailedLoginRepository>();

            // BoardRent services
            serviceCollection.AddSingleton<IAuthService, AuthService>();
            serviceCollection.AddSingleton<IAccountService, AccountService>();
            serviceCollection.AddSingleton<IAdminService, AdminService>();
            serviceCollection.AddSingleton<IFilePickerService, FilePickerService>();
            serviceCollection.AddSingleton<ISessionContext, SessionContext>();

            // BoardRent mappers (uniformity rule)
            serviceCollection.AddSingleton<AccountMapper>();
            serviceCollection.AddSingleton<AccountProfileMapper>();

            // BoardRent view models
            serviceCollection.AddTransient<LoginViewModel>();
            serviceCollection.AddTransient<RegisterViewModel>();
            serviceCollection.AddTransient<ProfileViewModel>();
            serviceCollection.AddTransient<AdminViewModel>();

            Services = serviceCollection.BuildServiceProvider();
            Ioc.Default.ConfigureServices(Services);
        }

        // Static helpers used by BoardRent view models that call App.NavigateTo / App.NavigateBack.
        public static void NavigateTo(Type pageType, object? parameter = null, bool clearBackStack = false)
        {
            if (Application.Current is not App appInstance)
            {
                return;
            }

            if (appInstance.RootFrame == null)
            {
                return;
            }

            appInstance.RootFrame.Navigate(pageType, parameter);
            if (clearBackStack)
            {
                appInstance.RootFrame.BackStack.Clear();
            }
        }

        public static void NavigateBack()
        {
            if (Application.Current is not App appInstance)
            {
                return;
            }

            if (appInstance.RootFrame != null && appInstance.RootFrame.CanGoBack)
            {
                appInstance.RootFrame.GoBack();
            }
        }

        private int GetProcessSlotFromArgs()
        {
            var commandLineArgs = Environment.GetCommandLineArgs();
            if (commandLineArgs.Length > ProcessSlotArgumentIndex
                && int.TryParse(commandLineArgs[ProcessSlotArgumentIndex], out var parsedProcessSlot))
            {
                return parsedProcessSlot;
            }

            return DefaultProcessSlot;
        }

        #region Two-window dev mode

        private static string? FindRepoRoot()
        {
            var currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);
            while (currentDirectory != null)
            {
                if (Directory.Exists(Path.Combine(currentDirectory.FullName, ".git")))
                {
                    return currentDirectory.FullName;
                }

                currentDirectory = currentDirectory.Parent;
            }
            return null;
        }

        private static string? FindNotificationServerBinDir()
        {
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                var candidate = Path.Combine(current.FullName, "NotificationServer", "bin");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }
            return null;
        }

        private static bool IsTwoWindowsEnabled()
        {
            try
            {
                var repoRoot = FindRepoRoot();
                if (repoRoot == null)
                {
                    return false;
                }

                var envPath = Path.Combine(repoRoot, ".env");
                if (!File.Exists(envPath))
                {
                    return false;
                }

                foreach (var line in File.ReadAllLines(envPath))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith('#') || !trimmed.Contains('='))
                    {
                        continue;
                    }

                    var parts = trimmed.Split('=', SplitKeyValuePartsCount);
                    if (parts[KeyPartIndex].Trim() == TwoWindowsEnvironmentKey)
                    {
                        return parts[ValuePartIndex].Trim().Equals(EnabledEnvironmentValue, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static void StartNotificationServer()
        {
            try
            {
                if (Process.GetProcessesByName("NotificationServer").Length > NoRunningProcessCount)
                {
                    return;
                }

                var serverBinDir = FindNotificationServerBinDir();
                if (serverBinDir == null)
                {
                    return;
                }

                var serverExe = Directory.GetFiles(serverBinDir, "NotificationServer.exe", SearchOption.AllDirectories)
                    .FirstOrDefault();
                if (serverExe == null)
                {
                    return;
                }

                notificationServerProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = serverExe,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Minimized,
                });
            }
            catch
            {
            }
        }

        private static void LaunchSecondClient()
        {
            try
            {
                var currentExe = Environment.ProcessPath;
                if (currentExe == null)
                {
                    return;
                }

                secondClientProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = currentExe,
                    Arguments = DevModeSecondaryProcessSlot.ToString(),
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(currentExe),
                });
            }
            catch
            {
            }
        }

        private static void KillSpawnedChildProcesses()
        {
            try
            {
                if (secondClientProcess != null && !secondClientProcess.HasExited)
                {
                    secondClientProcess.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }

            try
            {
                if (notificationServerProcess != null && !notificationServerProcess.HasExited)
                {
                    notificationServerProcess.Kill(entireProcessTree: true);
                }
            }
            catch
            {
            }
        }

        #endregion

        private void SetupNotificationManager()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                notificationManager.Unregister();
                (notificationService as IDisposable)?.Dispose();
                KillSpawnedChildProcesses();
            };

            notificationManager.NotificationClicked += (sender, args) =>
            {
                mainWindow?.DispatcherQueue.TryEnqueue(() =>
                {
                    mainWindow?.Activate();

                    if (args.Arguments.ContainsKey(NotificationNavigationArgumentKey)
                        && args.Arguments[NotificationNavigationArgumentKey] == nameof(NotificationsPage))
                    {
                        ActivateWindow();
                        NavigateToNotificationsWithinShell();
                    }
                });
            };

            notificationManager.Init();
        }

        private void NavigateToNotificationsWithinShell()
        {
            if (RootFrame?.Content is MenuBarPage currentShell)
            {
                currentShell.NavigateToNotifications();
                return;
            }

            void OnShellLoaded(object sender, NavigationEventArgs navigationEventArgs)
            {
                if (navigationEventArgs.Content is MenuBarPage loadedShell)
                {
                    RootFrame!.Navigated -= OnShellLoaded;
                    loadedShell.NavigateToNotifications();
                }
            }

            RootFrame!.Navigated += OnShellLoaded;
            RootFrame.Navigate(typeof(MenuBarPage), gameService);
        }

        private void EnsureSingleInstance(string appUserModelId)
        {
            var appInstance = AppInstance.FindOrRegisterForKey(appUserModelId);
            if (!appInstance.IsCurrent)
            {
                appInstance.RedirectActivationToAsync(AppInstance.GetCurrent().GetActivatedEventArgs()).AsTask().Wait();
                Environment.Exit(SuccessExitCode);
            }

            appInstance.Activated += (sender, args) => ActivateWindow();
        }

        private void InitializeServices()
        {
            RootFrame = new Frame();

            notificationRepository = Services.GetRequiredService<INotificationRepository>();
            notificationService = Services.GetRequiredService<INotificationService>();
            gameRepository = Services.GetRequiredService<IGameRepository>();
            gameService = Services.GetRequiredService<IGameService>();
            NotificationsViewModel = Services.GetRequiredService<NotificationsViewModel>();

            _ = notificationRepository;
            _ = gameRepository;

            // Listen continuously from app start; subscribing to a specific user is
            // deferred to OnUserLoggedIn so the server gets the right AccountId.
            notificationService.StartListening();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            CreateAndShowMainWindow();

            var rootGrid = new Grid();
            rootGrid.Children.Add(RootFrame);
            MainWindow!.Content = rootGrid;

            RootFrame!.Navigate(typeof(LoginPage));

            CreateTrayIcon();

            if (shouldLaunchSecondClient)
            {
                LaunchSecondClient();
            }
        }

        public static void OnUserLoggedIn()
        {
            if (Application.Current is not App appInstance)
            {
                return;
            }

            if (appInstance.RootFrame == null)
            {
                return;
            }

            var resolvedSessionContext = Services.GetRequiredService<ISessionContext>();
            var resolvedNotificationService = Services.GetRequiredService<INotificationService>();
            var resolvedMenuBarViewModel = Services.GetRequiredService<MenuBarViewModel>();
            var resolvedNotificationsViewModel = Services.GetRequiredService<NotificationsViewModel>();
            var resolvedGameService = Services.GetRequiredService<IGameService>();

            resolvedNotificationService.SubscribeToServer(resolvedSessionContext.AccountId);
            resolvedMenuBarViewModel.Rebuild();
            resolvedNotificationsViewModel.LoadNotificationsForUser(resolvedSessionContext.AccountId);

            NavigateTo(typeof(MenuBarPage), resolvedGameService, clearBackStack: true);
        }

        public static void OnUserLoggedOut()
        {
            if (Application.Current is not App appInstance)
            {
                return;
            }

            if (appInstance.RootFrame == null)
            {
                return;
            }

            var resolvedSessionContext = Services.GetRequiredService<ISessionContext>();
            resolvedSessionContext.Clear();

            NavigateTo(typeof(LoginPage), parameter: null, clearBackStack: true);
        }

        private void CreateAndShowMainWindow()
        {
            MainWindow = mainWindow = new MainWindow();
            mainWindow.Content = RootFrame;
            mainWindow.Activate();
            mainWindow.Title = AppUserModelId;
        }

        private void ActivateWindow()
        {
            mainWindow?.DispatcherQueue.TryEnqueue(() =>
            {
                if (mainWindow is MainWindow activatedMainWindow)
                {
                    activatedMainWindow.AppWindow.Show();
                }
                mainWindow?.Activate();
            });
        }

        private void CreateTrayIcon()
        {
            trayIcon = new TaskbarIcon
            {
                Id = CreateTrayIconId(CurrentProcessSlot),
                CustomName = $"{TrayIconIdentityPrefix}.Slot{CurrentProcessSlot}",
                ToolTipText = AppUserModelId,
                IconSource = new BitmapImage(new Uri(global::BoardRentAndProperty.Constants.App.AppTrayIconUri)),
            };

            var trayOpenCommand = new XamlUICommand();
            trayOpenCommand.ExecuteRequested += (sender, args) => ActivateWindow();
            var trayOpenMenuItem = new MenuFlyoutItem { Text = "Open", Command = trayOpenCommand };

            var trayExitCommand = new XamlUICommand();
            trayExitCommand.ExecuteRequested += (sender, args) =>
            {
                trayIcon.Dispose();
                Environment.Exit(SuccessExitCode);
            };
            var trayExitMenuItem = new MenuFlyoutItem { Text = "Exit", Command = trayExitCommand };

            trayIcon.ContextFlyout = new MenuFlyout { Items = { trayOpenMenuItem, trayExitMenuItem } };

            if (mainWindow!.Content is Grid rootGrid)
            {
                rootGrid.Children.Add(trayIcon);
            }
        }

        private static Guid CreateTrayIconId(int processSlot)
        {
            byte[] seedBytes = Encoding.UTF8.GetBytes($"{TrayIconIdentityPrefix}.Slot{processSlot}");
            byte[] hashBytes = MD5.HashData(seedBytes);
            return new Guid(hashBytes);
        }
    }
}
