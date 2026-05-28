namespace BoardGames.Desktop
{
    using System;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.UI.Xaml;
    using BoardGames.Desktop.Services;
    using BoardGames.Desktop.ViewModels;
    using BoardGames.Shared.ProxyServices;

    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public App()
        {
            this.InitializeComponent();

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient<IPaymentService, PaymentService>(c =>
                        c.BaseAddress = new Uri("http://localhost:5000/"));

                    services.AddSingleton<ISessionContext, SessionContext>();
                    services.AddSingleton<IDesktopAuthorizationService, DesktopAuthorizationService>();

                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<PaymentHistoryViewModel>();
                    services.AddTransient<ChatViewModel>();
                    services.AddTransient<ListingsViewModel>();
                    services.AddTransient<NotificationsViewModel>();
                    services.AddTransient<ProfileViewModel>();
                    services.AddTransient<AdminViewModel>();
                    services.AddTransient<MenuBarViewModel>();
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<RegisterViewModel>();
                    services.AddTransient<CreateGameViewModel>();
                    services.AddTransient<CreateRentalViewModel>();
                    services.AddTransient<CreateRequestViewModel>();
                    services.AddTransient<EditGameViewModel>();
                    services.AddTransient<RentalsFromOthersViewModel>();
                    services.AddTransient<RentalsToOthersViewModel>();
                    services.AddTransient<RequestsFromOthersViewModel>();
                    services.AddTransient<RequestsToOthersViewModel>();
                })
                .Build();

            Services = host.Services;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var window = new MainWindow();
            window.Activate();
        }
    }
}