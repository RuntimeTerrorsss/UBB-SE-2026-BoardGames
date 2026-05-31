using System;
using System.Collections.Generic;
using System.Linq;
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

        // DbSets from project 1
        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<City> Cities { get; set; }

        // DbSets from project 2
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

            // Composite key for ConversationParticipant
            modelBuilder.Entity<ConversationParticipant>()
                .HasKey(participant => new { participant.ConversationId, participant.UserId });

            // Message TPH hierarchy
            modelBuilder.Entity<Message>()
                .HasDiscriminator<string>("MessageCategory")
                .HasValue<TextMessage>("Text")
                .HasValue<ImageMessage>("Image")
                .HasValue<SystemMessage>("System")
                .HasValue<RentalRequestMessage>("RentalRequest")
                .HasValue<CashAgreementMessage>("CashAgreement");

            // Message → User relationships
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

            // Derived Message relationships
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

            // Conversation Participants → User
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
                    commaSeparatedNames => commaSeparatedNames.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (list1, list2) => list1.SequenceEqual(list2),
                    namesList => namesList.Aggregate(0, (accumulator, name) => HashCode.Combine(accumulator, name.GetHashCode())),
                    namesList => namesList.ToList()
                ));

            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = AdminRoleId,
                    Name = "Administrator",
                },
                new Role
                {
                    Id = StandardUserRoleId,
                    Name = "Standard User",
                });

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = AdminAccountId,
                    PamUserId = 4,
                    Username = "admin",
                    DisplayName = "Administrator",
                    Email = "admin@boardrent.com",
                    PasswordHash = SeedDevPasswordHash,
                    PhoneNumber = string.Empty,
                    AvatarUrl = string.Empty,
                    Country = string.Empty,
                    City = string.Empty,
                    StreetName = string.Empty,
                    StreetNumber = string.Empty,
                    IsSuspended = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                },
                new User
                {
                    Id = DariusAccountId,
                    PamUserId = 1,
                    Username = "darius",
                    DisplayName = "Darius Turcu",
                    Email = "darius@boardrent.com",
                    PasswordHash = SeedDevPasswordHash,
                    PhoneNumber = string.Empty,
                    AvatarUrl = string.Empty,
                    Country = string.Empty,
                    City = string.Empty,
                    StreetName = string.Empty,
                    StreetNumber = string.Empty,
                    IsSuspended = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                },
                new User
                {
                    Id = MihaiAccountId,
                    PamUserId = 2,
                    Username = "mihai",
                    DisplayName = "Mihai Tira",
                    Email = "mihai@boardrent.com",
                    PasswordHash = SeedDevPasswordHash,
                    PhoneNumber = string.Empty,
                    AvatarUrl = string.Empty,
                    Country = string.Empty,
                    City = string.Empty,
                    StreetName = string.Empty,
                    StreetNumber = string.Empty,
                    IsSuspended = false,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                });

            modelBuilder.Entity<AccountRole>().HasData(
                new AccountRole
                {
                    AccountId = AdminAccountId,
                    RoleId = AdminRoleId,
                },
                new AccountRole
                {
                    AccountId = DariusAccountId,
                    RoleId = StandardUserRoleId,
                },
                new AccountRole
                {
                    AccountId = MihaiAccountId,
                    RoleId = StandardUserRoleId,
                });
        }
    }
}