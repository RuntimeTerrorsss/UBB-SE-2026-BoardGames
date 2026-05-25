// <copyright file="AppDbContext.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BoardGames.Data
{
    public class AppDbContext : DbContext
    {
        private static readonly Guid AdminRoleId = new Guid("00000000-0000-0000-0000-000000000001");
        private static readonly Guid StandardUserRoleId = new Guid("00000000-0000-0000-0000-000000000002");
        private static readonly Guid AdminAccountId = new Guid("00000000-0000-0000-0000-000000000010");
        private static readonly Guid DariusAccountId = new Guid("00000000-0000-0000-0000-000000000011");
        private static readonly Guid MihaiAccountId = new Guid("00000000-0000-0000-0000-000000000012");

        private const string SeedDevPasswordHash = "uDsZUEmrma0uYI3Jszc4zA==:VX158vwbXUFhq/hkFoNOvOYZJgS5od0LYCbwn1dYF+8=";

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Rental> Rentals { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }

        public DbSet<City> Cities { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<AccountRole> AccountRoles { get; set; }

        public DbSet<FailedLoginAttempt> FailedLoginAttempts { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(user => user.Id);
                entity.Property(user => user.Username).HasMaxLength(100).IsRequired();
                entity.Property(user => user.DisplayName).HasMaxLength(200).IsRequired();
                entity.Property(user => user.Email).HasMaxLength(200).IsRequired();
                entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
                entity.Property(user => user.PhoneNumber).HasMaxLength(50);
                entity.Property(user => user.AvatarUrl).HasMaxLength(500);
                entity.Property(user => user.StreetName).HasMaxLength(200);
                entity.Property(user => user.StreetNumber).HasMaxLength(20);
                entity.Property(user => user.Country).HasMaxLength(100);
                entity.Property(user => user.City).HasMaxLength(100);
                entity.HasIndex(user => user.Username).IsUnique();
                entity.HasIndex(user => user.Email).IsUnique();

                entity.Property(user => user.PamUserId).IsRequired();
                entity.HasAlternateKey(user => user.PamUserId);

                entity.Property(user => user.Balance).HasPrecision(18, 2);

                entity.HasMany(user => user.Roles)
                      .WithMany()
                      .UsingEntity<AccountRole>(
                          joinEntity => joinEntity.HasOne(accountRole => accountRole.Role).WithMany().HasForeignKey(accountRole => accountRole.RoleId).OnDelete(DeleteBehavior.Cascade),
                          joinEntity => joinEntity.HasOne(accountRole => accountRole.Account).WithMany().HasForeignKey(accountRole => accountRole.AccountId).OnDelete(DeleteBehavior.Cascade),
                          joinEntity =>
                          {
                              joinEntity.ToTable("AccountRoles");
                              joinEntity.HasKey(accountRole => new { accountRole.AccountId, accountRole.RoleId });
                          });
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
                entity.HasKey(role => role.Id);
                entity.Property(role => role.Name).HasMaxLength(50).IsRequired();
                entity.HasIndex(role => role.Name).IsUnique();
            });

            modelBuilder.Entity<FailedLoginAttempt>(entity =>
            {
                entity.ToTable("FailedLoginAttempt");
                entity.HasKey(failedLogin => failedLogin.AccountId);
                entity.HasOne(failedLogin => failedLogin.Account)
                      .WithOne()
                      .HasForeignKey<FailedLoginAttempt>(failedLogin => failedLogin.AccountId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Game>(entity =>
            {
                entity.ToTable("Games");
                entity.HasKey(game => game.Id);
                entity.Property(game => game.Id).ValueGeneratedOnAdd();
                entity.Property(game => game.Name).HasMaxLength(100).IsRequired();
                entity.Property(game => game.PricePerDay).HasColumnType("decimal(10,2)");
                entity.Property(game => game.Image).HasColumnType("varbinary(max)");

                entity.HasOne(game => game.Owner)
                      .WithMany(user => user.OwnedGames)
                      .HasForeignKey(game => game.OwnerId)
                      .HasPrincipalKey(user => user.PamUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(game => game.IsActive)
                      .HasConversion<int>();
            });

            modelBuilder.Entity<Rental>(entity =>
            {
                entity.ToTable("Rentals");
                entity.HasKey(rental => rental.Id);
                entity.Property(rental => rental.Id).ValueGeneratedOnAdd();

                entity.HasOne(rental => rental.Game)
                      .WithMany(game => game.Rentals)
                      .HasForeignKey(rental => rental.GameId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(rental => rental.Client)
                      .WithMany(user => user.RentalsAsClient)
                      .HasForeignKey(rental => rental.ClientId)
                      .HasPrincipalKey(user => user.PamUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(rental => rental.Owner)
                      .WithMany(user => user.RentalsAsOwner)
                      .HasForeignKey(rental => rental.OwnerId)
                      .HasPrincipalKey(user => user.PamUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(rental => rental.TotalPrice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Payment>()
                .HasDiscriminator<string>("PaymentCategory")
                .HasValue<Payment>("Standard")
                .HasValue<HistoryPayment>("History");

            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.Client)
                .WithMany()
                .HasForeignKey(payment => payment.ClientId)
                .HasPrincipalKey(user => user.PamUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.Owner)
                .WithMany()
                .HasForeignKey(payment => payment.OwnerId)
                .HasPrincipalKey(user => user.PamUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.Request)
                .WithMany()
                .HasForeignKey(payment => payment.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .Property(payment => payment.PaidAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Requests");
                entity.HasKey(request => request.Id);
                entity.Property(request => request.Id).ValueGeneratedOnAdd();
                entity.HasOne(request => request.Game).WithMany().HasForeignKey("GameId").OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(request => request.Renter).WithMany().HasForeignKey("RenterId").HasPrincipalKey(user => user.PamUserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(request => request.Owner).WithMany().HasForeignKey("OwnerId").HasPrincipalKey(user => user.PamUserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(request => request.OfferingUser).WithMany().HasForeignKey("OfferingUserId").HasPrincipalKey(user => user.PamUserId).OnDelete(DeleteBehavior.SetNull).IsRequired(false);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(notification => notification.Id);
                entity.Property(notification => notification.Id)
                      .HasColumnName("notification_id")
                      .ValueGeneratedOnAdd();
                entity.Property(notification => notification.Title).HasMaxLength(200).IsRequired();
                entity.Property(notification => notification.Body).HasMaxLength(2000).IsRequired();

                entity.HasOne(notification => notification.Recipient)
                      .WithMany()
                      .HasForeignKey("user_id")
                      .HasPrincipalKey(user => user.PamUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(notification => notification.RelatedRequest)
                      .WithMany()
                      .HasForeignKey("related_request_id")
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);
            });

            modelBuilder.Entity<ConversationParticipant>()
                .HasKey(participant => new { participant.ConversationId, participant.UserId });

            modelBuilder.Entity<Message>()
                .HasDiscriminator<string>("MessageCategory")
                .HasValue<TextMessage>("Text")
                .HasValue<ImageMessage>("Image")
                .HasValue<SystemMessage>("System")
                .HasValue<RentalRequestMessage>("RentalRequest")
                .HasValue<CashAgreementMessage>("CashAgreement");

            modelBuilder.Entity<Message>()
                .HasOne(message => message.Sender)
                .WithMany()
                .HasForeignKey(message => message.MessageSenderId)
                .HasPrincipalKey(user => user.PamUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(message => message.Receiver)
                .WithMany()
                .HasForeignKey(message => message.MessageReceiverId)
                .HasPrincipalKey(user => user.PamUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RentalRequestMessage>()
                .HasOne(message => message.RentalRequest)
                .WithMany(rental => rental.Messages)
                .HasForeignKey(message => message.RentalRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CashAgreementMessage>()
                .HasOne(message => message.CashPayment)
                .WithMany()
                .HasForeignKey(message => message.CashPaymentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ConversationParticipant>()
                .HasOne(participant => participant.User)
                .WithMany(user => user.Conversations)
                .HasForeignKey(participant => participant.UserId)
                .HasPrincipalKey(user => user.PamUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<City>()
                .Property(city => city.Names)
                .HasConversion(
                    namesList => string.Join(',', namesList),
                    commaSeparatedNames => commaSeparatedNames.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (list1, list2) => list1.SequenceEqual(list2),
                    namesList => namesList.Aggregate(0, (accumulator, name) => HashCode.Combine(accumulator, name.GetHashCode())),
                    namesList => namesList.ToList()));

            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = AdminRoleId, Name = "Administrator" },
                new Role { Id = StandardUserRoleId, Name = "Standard User" }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = AdminAccountId,
                    PamUserId = 4,
                    Username = "admin",
                    DisplayName = "Administrator",
                    Email = "admin@boardrent.com",
                    PasswordHash = SeedDevPasswordHash,
                    PhoneNumber = "",
                    AvatarUrl = "",
                    Country = "",
                    City = "",
                    StreetName = "",
                    StreetNumber = "",
                    IsSuspended = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    Balance = 0
                },
                new User
                {
                    Id = DariusAccountId,
                    PamUserId = 1,
                    Username = "darius",
                    DisplayName = "Darius Turcu",
                    Email = "darius@boardrent.com",
                    PasswordHash = SeedDevPasswordHash,
                    IsSuspended = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    Balance = 150
                },
                new User
                {
                    Id = MihaiAccountId,
                    PamUserId = 2,
                    Username = "mihai",
                    DisplayName = "Mihai Tira",
                    Email = "mihai@boardrent.com",
                    PasswordHash = SeedDevPasswordHash,
                    IsSuspended = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    Balance = 75.5m
                },
                new User
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    PamUserId = 3,
                    Username = "alice01",
                    DisplayName = "Alice",
                    Email = "alice@example.com",
                    PasswordHash = "hash1",
                    Balance = 150,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new User
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    PamUserId = 5,
                    Username = "bob02",
                    DisplayName = "Bob",
                    Email = "bob@example.com",
                    PasswordHash = "hash2",
                    Balance = 75.5m,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new User
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    PamUserId = 6,
                    Username = "carol03",
                    DisplayName = "Carol",
                    Email = "carol@example.com",
                    PasswordHash = "hash3",
                    Balance = 200,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new User
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    PamUserId = 7,
                    Username = "david04",
                    DisplayName = "David",
                    Email = "david@example.com",
                    PasswordHash = "hash4",
                    Balance = 50,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                },
                new User
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    PamUserId = 8,
                    Username = "emma05",
                    DisplayName = "Emma",
                    Email = "emma@example.com",
                    PasswordHash = "hash5",
                    Balance = 320,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate
                }
            );

            modelBuilder.Entity<AccountRole>().HasData(
                new AccountRole { AccountId = AdminAccountId, RoleId = AdminRoleId },
                new AccountRole { AccountId = DariusAccountId, RoleId = StandardUserRoleId },
                new AccountRole { AccountId = MihaiAccountId, RoleId = StandardUserRoleId },
                new AccountRole { AccountId = Guid.Parse("11111111-1111-1111-1111-111111111111"), RoleId = StandardUserRoleId },
                new AccountRole { AccountId = Guid.Parse("22222222-2222-2222-2222-222222222222"), RoleId = StandardUserRoleId },
                new AccountRole { AccountId = Guid.Parse("33333333-3333-3333-3333-333333333333"), RoleId = StandardUserRoleId },
                new AccountRole { AccountId = Guid.Parse("44444444-4444-4444-4444-444444444444"), RoleId = StandardUserRoleId },
                new AccountRole { AccountId = Guid.Parse("55555555-5555-5555-5555-555555555555"), RoleId = StandardUserRoleId }
            );

            modelBuilder.Entity<Game>().HasData(
                new Game { Id = 1, Name = "Catan", PricePerDay = 15, MinimumPlayerNumber = 3, MaximumPlayerNumber = 4, Description = "Trade and build on the island of Catan.", IsActive = true, OwnerId = 1 },
                new Game { Id = 2, Name = "Monopoly", PricePerDay = 10, MinimumPlayerNumber = 2, MaximumPlayerNumber = 6, Description = "Classic property trading game.", IsActive = true, OwnerId = 3 },
                new Game { Id = 3, Name = "Carcassonne", PricePerDay = 12.5m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 5, Description = "Tile placement game.", IsActive = true, OwnerId = 1 },
                new Game { Id = 4, Name = "Terraforming Mars", PricePerDay = 20, MinimumPlayerNumber = 1, MaximumPlayerNumber = 5, Description = "Strategy game about developing Mars.", IsActive = false, OwnerId = 3 },
                new Game { Id = 5, Name = "Ticket to Ride", PricePerDay = 13.5m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 5, Description = "Build railway routes across the world.", IsActive = true, OwnerId = 1 },
                new Game { Id = 6, Name = "Pandemic", PricePerDay = 14, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Work together to stop global outbreaks.", IsActive = true, OwnerId = 2 },
                new Game { Id = 7, Name = "7 Wonders", PricePerDay = 16, MinimumPlayerNumber = 2, MaximumPlayerNumber = 7, Description = "Build a civilization and wonders.", IsActive = true, OwnerId = 3 },
                new Game { Id = 8, Name = "Azul", PricePerDay = 11, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Decorate the royal palace walls.", IsActive = true, OwnerId = 1 },
                new Game { Id = 9, Name = "Dixit", PricePerDay = 10.5m, MinimumPlayerNumber = 3, MaximumPlayerNumber = 6, Description = "Creative storytelling game.", IsActive = true, OwnerId = 2 },
                new Game { Id = 10, Name = "Splendor", PricePerDay = 12, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Build your gem empire.", IsActive = true, OwnerId = 8 },
                new Game { Id = 11, Name = "Codenames", PricePerDay = 9, MinimumPlayerNumber = 2, MaximumPlayerNumber = 8, Description = "Team word guessing game.", IsActive = true, OwnerId = 6 },
                new Game { Id = 12, Name = "Risk", PricePerDay = 11.5m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 6, Description = "Classic world domination game.", IsActive = true, OwnerId = 5 },
                new Game { Id = 13, Name = "Dominion", PricePerDay = 13, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Deck-building strategy game.", IsActive = true, OwnerId = 7 },
                new Game { Id = 14, Name = "Love Letter", PricePerDay = 7.5m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Quick deduction card game.", IsActive = true, OwnerId = 8 },
                new Game { Id = 15, Name = "Scythe", PricePerDay = 22, MinimumPlayerNumber = 1, MaximumPlayerNumber = 5, Description = "Strategy game in alternate history.", IsActive = true, OwnerId = 2 },
                new Game { Id = 16, Name = "Wingspan", PricePerDay = 18, MinimumPlayerNumber = 1, MaximumPlayerNumber = 5, Description = "Build a bird sanctuary.", IsActive = true, OwnerId = 3 },
                new Game { Id = 17, Name = "Gloomhaven", PricePerDay = 25, MinimumPlayerNumber = 1, MaximumPlayerNumber = 4, Description = "Epic campaign dungeon crawler.", IsActive = true, OwnerId = 1 },
                new Game { Id = 18, Name = "Brass Birmingham", PricePerDay = 21, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Industrial revolution strategy.", IsActive = true, OwnerId = 8 },
                new Game { Id = 19, Name = "Root", PricePerDay = 17.5m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 4, Description = "Asymmetric woodland warfare.", IsActive = true, OwnerId = 8 },
                new Game { Id = 20, Name = "Terraforming Mars: Ares", PricePerDay = 19, MinimumPlayerNumber = 1, MaximumPlayerNumber = 4, Description = "Faster Mars engine builder.", IsActive = true, OwnerId = 7 },
                new Game { Id = 21, Name = "Ark Nova", PricePerDay = 23, MinimumPlayerNumber = 1, MaximumPlayerNumber = 4, Description = "Build the best zoo.", IsActive = true, OwnerId = 6 },
                new Game { Id = 22, Name = "Everdell", PricePerDay = 16.5m, MinimumPlayerNumber = 1, MaximumPlayerNumber = 4, Description = "Build a forest civilization.", IsActive = true, OwnerId = 6 },
                new Game { Id = 23, Name = "The Crew", PricePerDay = 9.5m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 5, Description = "Cooperative trick-taking game.", IsActive = true, OwnerId = 4 },
                new Game { Id = 24, Name = "Hanabi", PricePerDay = 8, MinimumPlayerNumber = 2, MaximumPlayerNumber = 5, Description = "Play cards without seeing them.", IsActive = true, OwnerId = 4 },
                new Game { Id = 25, Name = "Agricola", PricePerDay = 17, MinimumPlayerNumber = 1, MaximumPlayerNumber = 4, Description = "Farm-building strategy game.", IsActive = true, OwnerId = 4 },
                new Game { Id = 26, Name = "Patchwork", PricePerDay = 10, MinimumPlayerNumber = 2, MaximumPlayerNumber = 2, Description = "Two-player quilt game.", IsActive = true, OwnerId = 5 },
                new Game { Id = 27, Name = "Carcassonne: Expansion", PricePerDay = 13.5m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 6, Description = "Expand the classic Carcassonne.", IsActive = true, OwnerId = 6 },
                new Game { Id = 28, Name = "Uno", PricePerDay = 5, MinimumPlayerNumber = 2, MaximumPlayerNumber = 6, Description = "Classic card shedding game.", IsActive = true, OwnerId = 3 },
                new Game { Id = 29, Name = "Exploding Kittens", PricePerDay = 8.5m, MinimumPlayerNumber = 2, MaximumPlayerNumber = 5, Description = "Explosive card game.", IsActive = true, OwnerId = 1 },
                new Game { Id = 30, Name = "Bang!", PricePerDay = 9, MinimumPlayerNumber = 4, MaximumPlayerNumber = 7, Description = "Wild west bluffing game.", IsActive = true, OwnerId = 2 }
            );

            modelBuilder.Entity<Rental>().HasData(
                new Rental { Id = 1, GameId = 1, ClientId = 2, OwnerId = 1, StartDate = new DateTime(2026, 5, 10), EndDate = new DateTime(2026, 5, 15), TotalPrice = 75 },
                new Rental { Id = 2, GameId = 2, ClientId = 1, OwnerId = 3, StartDate = new DateTime(2026, 5, 12), EndDate = new DateTime(2026, 5, 14), TotalPrice = 20 },
                new Rental { Id = 3, GameId = 1, ClientId = 4, OwnerId = 1, StartDate = new DateTime(2026, 4, 1), EndDate = new DateTime(2026, 4, 5), TotalPrice = 20 },
                new Rental { Id = 4, GameId = 5, ClientId = 5, OwnerId = 1, StartDate = new DateTime(2026, 5, 1), EndDate = new DateTime(2026, 5, 10), TotalPrice = 135 },
                new Rental { Id = 5, GameId = 7, ClientId = 6, OwnerId = 3, StartDate = new DateTime(2026, 4, 15), EndDate = new DateTime(2026, 4, 18), TotalPrice = 48 },
                new Rental { Id = 6, GameId = 12, ClientId = 7, OwnerId = 5, StartDate = new DateTime(2026, 5, 1), EndDate = new DateTime(2026, 5, 7), TotalPrice = 69 },
                new Rental { Id = 7, GameId = 23, ClientId = 2, OwnerId = 4, StartDate = new DateTime(2026, 5, 15), EndDate = new DateTime(2026, 5, 17), TotalPrice = 19 }
            );

            modelBuilder.Entity<Payment>().HasData(
                new Payment { TransactionIdentifier = 1, RequestId = 1, ClientId = 2, OwnerId = 1, PaidAmount = 75, PaymentMethod = "CARD", DateOfTransaction = new DateTime(2026, 5, 1, 10, 0, 0), DateConfirmedBuyer = new DateTime(2026, 5, 1, 10, 0, 0), PaymentState = 1 },
                new Payment { TransactionIdentifier = 2, RequestId = 2, ClientId = 1, OwnerId = 3, PaidAmount = 20, PaymentMethod = "CASH", DateOfTransaction = new DateTime(2026, 5, 10, 14, 30, 0), PaymentState = 1 },
                new Payment { TransactionIdentifier = 3, RequestId = 3, ClientId = 4, OwnerId = 1, PaidAmount = 20, PaymentMethod = "CARD", DateOfTransaction = new DateTime(2026, 3, 25, 9, 0, 0), DateConfirmedBuyer = new DateTime(2026, 3, 25, 9, 0, 0), PaymentState = 0 },
                new Payment { TransactionIdentifier = 4, RequestId = 4, ClientId = 5, OwnerId = 1, PaidAmount = 135, PaymentMethod = "CASH", DateOfTransaction = new DateTime(2026, 4, 25, 8, 0, 0), PaymentState = 1 },
                new Payment { TransactionIdentifier = 5, RequestId = 5, ClientId = 6, OwnerId = 3, PaidAmount = 48, PaymentMethod = "CARD", DateOfTransaction = new DateTime(2026, 4, 10, 11, 0, 0), DateConfirmedBuyer = new DateTime(2026, 4, 10, 11, 0, 0), PaymentState = 1 },
                new Payment { TransactionIdentifier = 6, RequestId = 6, ClientId = 7, OwnerId = 5, PaidAmount = 69, PaymentMethod = "CASH", DateOfTransaction = new DateTime(2026, 4, 25, 16, 0, 0), PaymentState = 0 },
                new Payment { TransactionIdentifier = 7, RequestId = 7, ClientId = 2, OwnerId = 4, PaidAmount = 19, PaymentMethod = "CARD", DateOfTransaction = new DateTime(2026, 5, 10, 10, 0, 0), DateConfirmedBuyer = new DateTime(2026, 5, 10, 10, 0, 0), DateConfirmedSeller = new DateTime(2026, 5, 10, 10, 0, 0), PaymentState = 1 }
            );

            for (int i = 1; i <= 7; i++)
                modelBuilder.Entity<Conversation>().HasData(new Conversation { ConversationId = i });

            modelBuilder.Entity<ConversationParticipant>().HasData(
                new ConversationParticipant { ConversationId = 1, UserId = 1, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 1, UserId = 2, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 2, UserId = 3, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 2, UserId = 1, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 3, UserId = 1, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 3, UserId = 4, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 4, UserId = 1, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 4, UserId = 5, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 5, UserId = 3, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 5, UserId = 6, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 6, UserId = 5, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 6, UserId = 7, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 7, UserId = 4, LastMessageReadTime = null, UnreadMessagesCount = 0 },
                new ConversationParticipant { ConversationId = 7, UserId = 2, LastMessageReadTime = null, UnreadMessagesCount = 0 }
            );

            modelBuilder.Entity<Message>().HasData(
                new RentalRequestMessage
                {
                    MessageId = 1,
                    ConversationId = 1,
                    MessageSenderId = 2,
                    MessageReceiverId = 1,
                    MessageSentTime = new DateTime(2026, 4, 1, 9, 0, 0),
                    MessageContentAsString = "Hey, is Catan available May 10-15?",
                    RentalRequestId = 1,
                    IsRequestResolved = true,
                    IsRequestAccepted = true,
                    RequestContent = "Hey, is Catan available May 10-15?"
                },
                new TextMessage
                {
                    MessageId = 2,
                    ConversationId = 1,
                    MessageSenderId = 1,
                    MessageReceiverId = 2,
                    MessageSentTime = new DateTime(2026, 4, 1, 9, 5, 0),
                    MessageContentAsString = "Yes, it's free — it's all yours!",
                    TextMessageContent = "Yes, it's free — it's all yours!"
                },
                new ImageMessage
                {
                    MessageId = 3,
                    ConversationId = 1,
                    MessageSenderId = 2,
                    MessageReceiverId = 1,
                    MessageSentTime = new DateTime(2026, 4, 1, 9, 8, 0),
                    MessageContentAsString = "hamster.jpg",
                    MessageImageUrl = "hamster.jpg"
                },
                new TextMessage
                {
                    MessageId = 4,
                    ConversationId = 1,
                    MessageSenderId = 2,
                    MessageReceiverId = 1,
                    MessageSentTime = new DateTime(2026, 4, 1, 9, 10, 0),
                    MessageContentAsString = "Perfect, thanks a lot!",
                    TextMessageContent = "Perfect, thanks a lot!"
                },
                new RentalRequestMessage
                {
                    MessageId = 5,
                    ConversationId = 2,
                    MessageSenderId = 1,
                    MessageReceiverId = 3,
                    MessageSentTime = new DateTime(2026, 5, 5, 10, 0, 0),
                    MessageContentAsString = "Can I borrow Monopoly May 12-14?",
                    RentalRequestId = 2,
                    IsRequestResolved = true,
                    IsRequestAccepted = true,
                    RequestContent = "Can I borrow Monopoly May 12-14?"
                },
                new TextMessage
                {
                    MessageId = 6,
                    ConversationId = 2,
                    MessageSenderId = 3,
                    MessageReceiverId = 1,
                    MessageSentTime = new DateTime(2026, 5, 5, 10, 10, 0),
                    MessageContentAsString = "Sure, I can bring it over Monday.",
                    TextMessageContent = "Sure, I can bring it over Monday."
                },
                new TextMessage
                {
                    MessageId = 7,
                    ConversationId = 2,
                    MessageSenderId = 1,
                    MessageReceiverId = 3,
                    MessageSentTime = new DateTime(2026, 5, 5, 10, 15, 0),
                    MessageContentAsString = "Great, see you then!",
                    TextMessageContent = "Great, see you then!"
                },
                new RentalRequestMessage
                {
                    MessageId = 8,
                    ConversationId = 3,
                    MessageSenderId = 4,
                    MessageReceiverId = 1,
                    MessageSentTime = new DateTime(2026, 3, 20, 9, 0, 0),
                    MessageContentAsString = "Hi, is Catan free from the 1st of April?",
                    RentalRequestId = 3,
                    IsRequestResolved = true,
                    IsRequestAccepted = true,
                    RequestContent = "Hi, is Catan free from the 1st of April?"
                },
                new TextMessage
                {
                    MessageId = 9,
                    ConversationId = 3,
                    MessageSenderId = 1,
                    MessageReceiverId = 4,
                    MessageSentTime = new DateTime(2026, 3, 20, 9, 5, 0),
                    MessageContentAsString = "Of course, come pick it up anytime.",
                    TextMessageContent = "Of course, come pick it up anytime."
                },
                new ImageMessage
                {
                    MessageId = 10,
                    ConversationId = 3,
                    MessageSenderId = 4,
                    MessageReceiverId = 1,
                    MessageSentTime = new DateTime(2026, 3, 20, 9, 8, 0),
                    MessageContentAsString = "hamster.jpg",
                    MessageImageUrl = "hamster.jpg"
                },
                new TextMessage
                {
                    MessageId = 11,
                    ConversationId = 3,
                    MessageSenderId = 1,
                    MessageReceiverId = 4,
                    MessageSentTime = new DateTime(2026, 3, 20, 9, 12, 0),
                    MessageContentAsString = "Will be there Tuesday morning!",
                    TextMessageContent = "Will be there Tuesday morning!"
                },
                new RentalRequestMessage
                {
                    MessageId = 12,
                    ConversationId = 4,
                    MessageSenderId = 5,
                    MessageReceiverId = 1,
                    MessageSentTime = new DateTime(2026, 4, 20, 8, 0, 0),
                    MessageContentAsString = "Would love to rent Ticket to Ride.",
                    RentalRequestId = 4,
                    IsRequestResolved = true,
                    IsRequestAccepted = true,
                    RequestContent = "Would love to rent Ticket to Ride."
                },
                new TextMessage
                {
                    MessageId = 13,
                    ConversationId = 4,
                    MessageSenderId = 1,
                    MessageReceiverId = 5,
                    MessageSentTime = new DateTime(2026, 4, 20, 8, 10, 0),
                    MessageContentAsString = "Sure, it's available. Want to meet Saturday?",
                    TextMessageContent = "Sure, it's available. Want to meet Saturday?"
                },
                new TextMessage
                {
                    MessageId = 14,
                    ConversationId = 4,
                    MessageSenderId = 5,
                    MessageReceiverId = 1,
                    MessageSentTime = new DateTime(2026, 4, 20, 8, 20, 0),
                    MessageContentAsString = "Saturday works perfectly for me.",
                    TextMessageContent = "Saturday works perfectly for me."
                },
                new RentalRequestMessage
                {
                    MessageId = 15,
                    ConversationId = 5,
                    MessageSenderId = 6,
                    MessageReceiverId = 3,
                    MessageSentTime = new DateTime(2026, 4, 1, 11, 0, 0),
                    MessageContentAsString = "Is 7 Wonders available?",
                    RentalRequestId = 5,
                    IsRequestResolved = true,
                    IsRequestAccepted = true,
                    RequestContent = "Is 7 Wonders available?"
                },
                new TextMessage
                {
                    MessageId = 16,
                    ConversationId = 5,
                    MessageSenderId = 3,
                    MessageReceiverId = 6,
                    MessageSentTime = new DateTime(2026, 4, 1, 11, 5, 0),
                    MessageContentAsString = "Yep, I'll have it ready by Tuesday.",
                    TextMessageContent = "Yep, I'll have it ready by Tuesday."
                },
                new ImageMessage
                {
                    MessageId = 17,
                    ConversationId = 5,
                    MessageSenderId = 6,
                    MessageReceiverId = 3,
                    MessageSentTime = new DateTime(2026, 4, 1, 11, 10, 0),
                    MessageContentAsString = "hamster.jpg",
                    MessageImageUrl = "hamster.jpg"
                },
                new RentalRequestMessage
                {
                    MessageId = 18,
                    ConversationId = 6,
                    MessageSenderId = 7,
                    MessageReceiverId = 5,
                    MessageSentTime = new DateTime(2026, 4, 15, 16, 0, 0),
                    MessageContentAsString = "Can I get Risk from May 1st to 7th?",
                    RentalRequestId = 6,
                    IsRequestResolved = true,
                    IsRequestAccepted = true,
                    RequestContent = "Can I get Risk from May 1st to 7th?"
                },
                new TextMessage
                {
                    MessageId = 19,
                    ConversationId = 6,
                    MessageSenderId = 5,
                    MessageReceiverId = 7,
                    MessageSentTime = new DateTime(2026, 4, 15, 16, 10, 0),
                    MessageContentAsString = "Sounds good, just message me before you come.",
                    TextMessageContent = "Sounds good, just message me before you come."
                },
                new TextMessage
                {
                    MessageId = 20,
                    ConversationId = 6,
                    MessageSenderId = 7,
                    MessageReceiverId = 5,
                    MessageSentTime = new DateTime(2026, 4, 15, 16, 20, 0),
                    MessageContentAsString = "Will do, cheers!",
                    TextMessageContent = "Will do, cheers!"
                },
                new RentalRequestMessage
                {
                    MessageId = 21,
                    ConversationId = 7,
                    MessageSenderId = 2,
                    MessageReceiverId = 4,
                    MessageSentTime = new DateTime(2026, 5, 8, 9, 0, 0),
                    MessageContentAsString = "Is The Crew free?",
                    RentalRequestId = 7,
                    IsRequestResolved = true,
                    IsRequestAccepted = true,
                    RequestContent = "Is The Crew free?"
                },
                new TextMessage
                {
                    MessageId = 22,
                    ConversationId = 7,
                    MessageSenderId = 4,
                    MessageReceiverId = 2,
                    MessageSentTime = new DateTime(2026, 5, 8, 9, 10, 0),
                    MessageContentAsString = "Yes, grab it.",
                    TextMessageContent = "Yes, grab it."
                }
            );
        }
    }
}