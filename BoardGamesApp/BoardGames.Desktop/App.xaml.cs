// <copyright file="App.xaml.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using System.Configuration;
using System.Net;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.Services.Listeners;
using BoardGames.Desktop.ViewModels;
using BoardGames.Desktop.Views;
using BoardGames.Shared.ProxyServices;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BoardGames.Desktop
{
    public partial class App : Application
    {
        private const int ApiClientTimeoutSeconds = 30;

        private Frame? rootFrame;

        public App()
        {
            ConfigureServices();
            this.InitializeComponent();
        }

        public static IServiceProvider Services { get; private set; } = default!;

        public static MainWindow MainWindow { get; private set; } = default!;

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            rootFrame = new Frame();
            MainWindow = new MainWindow();
            MainWindow.SetRootContent(rootFrame);
            MainWindow.Activate();
            rootFrame.Navigate(typeof(ShellPage));
        }

        public static void NavigateTo(AppPage route, object? parameter = null, bool clearBackStack = false)
        {
            if (TryGetShellPage(out var shellPage))
            {
                shellPage.NavigateTo(route, parameter, clearBackStack);
            }
        }

        public static void NavigateBack()
        {
            if (TryGetShellPage(out var shellPage))
            {
                shellPage.NavigateBack();
            }
        }

        public static void OnUserLoggedIn()
        {
            if (TryGetShellPage(out var shellPage))
            {
                shellPage.RefreshNavigation();
                shellPage.NavigateTo(AppPage.Filter, clearBackStack: true);
            }
        }

        public static void OnUserLoggedOut()
        {
            var sessionContext = Services.GetRequiredService<ISessionContext>();
            sessionContext.Clear();

            if (TryGetShellPage(out var shellPage))
            {
                shellPage.RefreshNavigation();
                shellPage.NavigateTo(AppPage.Filter, clearBackStack: true);
            }
        }

        private static bool TryGetShellPage(out ShellPage? shellPage)
        {
            shellPage = null;

            if (Current is not App app || app.rootFrame?.Content is not ShellPage currentShell)
            {
                return false;
            }

            shellPage = currentShell;
            return true;
        }

        private static void ConfigureServices()
        {
            string? apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]?.Trim();
            if (string.IsNullOrWhiteSpace(apiBaseUrl) || !Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiBaseAddress))
            {
                throw new InvalidOperationException("ApiBaseUrl is not configured correctly in App.config.");
            }

            var serviceCollection = new ServiceCollection();
            var cookieContainer = new CookieContainer();

            serviceCollection.AddBoardRentApiClient(options =>
            {
                options.BaseAddress = apiBaseAddress;
                options.Timeout = TimeSpan.FromSeconds(ApiClientTimeoutSeconds);
                options.CookieContainer = cookieContainer;
            });

            serviceCollection.AddSingleton(cookieContainer);
            serviceCollection.AddSingleton<ISessionContext, SessionContext>();
            serviceCollection.AddSingleton<ICurrentUserContext, CurrentUserContext>();
            serviceCollection.AddSingleton<IDesktopAuthorizationService, DesktopAuthorizationService>();
            serviceCollection.AddSingleton<IFilePickerService, FilePickerService>();
            serviceCollection.AddSingleton<IServerClient, NotificationClient>();
            serviceCollection.AddSingleton<IToastNotificationService, ToastNotificationService>();
            serviceCollection.AddSingleton<IDesktopNotificationService, DesktopNotificationService>();

            serviceCollection.AddSingleton<ShellViewModel>();
            serviceCollection.AddTransient<SearchGamesViewModel>();
            serviceCollection.AddTransient<GameDetailsPageViewModel>();
            serviceCollection.AddTransient<LoginViewModel>();
            serviceCollection.AddTransient<RegisterViewModel>();
            serviceCollection.AddTransient<DashboardViewModel>();
            serviceCollection.AddTransient<ChatViewModel>();
            serviceCollection.AddTransient<ListingsViewModel>();
            serviceCollection.AddTransient<NotificationsViewModel>();
            serviceCollection.AddTransient<ProfileViewModel>();
            serviceCollection.AddTransient<AdminViewModel>();
            serviceCollection.AddTransient<CreateGameViewModel>();
            serviceCollection.AddTransient<EditGameViewModel>();

            Services = serviceCollection.BuildServiceProvider();
            Ioc.Default.ConfigureServices(Services);
        }
    }
}
