// <copyright file="App.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using BoardGames.Desktop;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;
using BookingBoardGames.Sharing.Mapper;
using BookingBoardGames.Sharing.Repositories;
using BookingBoardGames.Sharing.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;

namespace BookingBoardGames
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static readonly string BaseApiUrl = "http://localhost:5000/api/";
        public static readonly string RemoteApiUrl = "http://172.30.250.124:5000/api/";

        public static readonly System.Net.Http.HttpClient Client = new System.Net.Http.HttpClient { BaseAddress = new Uri(RemoteApiUrl) };
        private Window? window;

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(DatabaseConfig.ResolveConnectionString())
                .Options;

            AppDbContext = new AppDbContext(options);

            // Repositories
            UserRepository = new UserAPIProxy(Client);
            GameRepository = new GamesAPIProxy(Client);
            RentalRepository = new RentalAPIProxy(Client);
            PaymentRepository = new PaymentAPIProxy(Client);
            HistoryRepository = new RepositoryPaymentAPIProxy(Client);
            ConversationRepository = new ConversationAPIProxy(Client);

            // Services
            ConversationNotifier = new ConversationNotifier();
            GlobalGeographicalService = new GeographicalService();
            RentalService = new RentalService(RentalRepository, GameRepository);
            ReceiptService = new ReceiptService(UserRepository, RentalService, GameRepository);
            CardPaymentService = new CardPaymentService(PaymentRepository, UserRepository, ReceiptService, RentalService);
            MapService = new MapService();
            var conversationService = new ConversationService(ConversationRepository, UserRepository, ConversationNotifier);
            ServicePayment = new ServicePayment(HistoryRepository, ReceiptService, RentalService, conversationService);
            CashPaymentService = new CashPaymentService(PaymentRepository, new CashPaymentMapper(), ReceiptService);
            BookingService = new BookingService(GameRepository, RentalRepository, UserRepository);
            SearchAndFilterService = new SearchAndFilterService(GameRepository, UserRepository, RentalRepository, GlobalGeographicalService);
            UserService = new UserService(UserRepository);
        }

        // AppDbContext
        public static AppDbContext? AppDbContext { get; private set; }

        // Repositories
        public static IUserRepository? UserRepository { get; private set; }

        public static InterfaceGamesRepository? GameRepository { get; private set; }

        public static IRentalRepository? RentalRepository { get; private set; }

        public static IPaymentRepository? PaymentRepository { get; private set; }

        public static IRepositoryPayment? HistoryRepository { get; private set; }

        public static IConversationRepository? ConversationRepository { get; private set; }

        // Services
        public static SessionService Session { get; private set; } = new SessionService();

        public static IConversationNotifier? ConversationNotifier { get; private set; }

        public static InterfaceGeographicalService? GlobalGeographicalService { get; private set; }

        public static IRentalService? RentalService { get; private set; }

        public static IReceiptService? ReceiptService { get; private set; }

        public static ICardPaymentService? CardPaymentService { get; private set; }

        public static IMapService? MapService { get; private set; }

        public static IServicePayment? ServicePayment { get; private set; }

        public static ICashPaymentService? CashPaymentService { get; private set; }

        public static InterfaceBookingService? BookingService { get; private set; }

        public static InterfaceSearchAndFilterService? SearchAndFilterService { get; private set; }

        public static ConversationService? ActiveConversationService { get; set; }
        public static IUserService? UserService { get; set; }

        public int DashboardUser { get; set; } = 3;

        public int NoChatsUser { get; set; } = 8;

        public Window? Window => this.window;

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            DatabaseBootstrap.Initialize();

            this.window = new MainWindow();
            this.window.Activate();

            try
            {
                if (GlobalGeographicalService == null)
                {
                    GlobalGeographicalService = new GeographicalService();
                }

                await GlobalGeographicalService.LoadCitiesFromFileAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GeographicalService initialization failed: {ex.Message}");
            }
        }
    }
}
