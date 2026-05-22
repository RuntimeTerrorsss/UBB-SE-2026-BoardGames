using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BoardGames.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // COMPOSITE KEYS
            modelBuilder.Entity<ConversationParticipant>()
                .HasKey(participant => new { participant.ConversationId, participant.UserId });


            // TABLE-PER-HIERARCHY (TPH) CONFIGURATIONS

            // Map the Message hierarchy
            modelBuilder.Entity<Message>()
                .HasDiscriminator<string>("MessageCategory")
                .HasValue<TextMessage>("Text")
                .HasValue<ImageMessage>("Image")
                .HasValue<SystemMessage>("System")
                .HasValue<RentalRequestMessage>("RentalRequest")
                .HasValue<CashAgreementMessage>("CashAgreement");

            // Map the Payment hierarchy
            modelBuilder.Entity<Payment>()
                .HasDiscriminator<string>("PaymentCategory")
                .HasValue<Payment>("Standard")
                .HasValue<HistoryPayment>("History");


            // FOREIGN KEYS & CASCADE DELETE PREVENTION

            // RENTALS
            modelBuilder.Entity<Rental>()
                .HasOne(rental => rental.Client)
                .WithMany(user => user.RentalsAsClient)
                .HasForeignKey(rental => rental.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Rental>()
                .HasOne(rental => rental.Owner)
                .WithMany(user => user.RentalsAsOwner)
                .HasForeignKey(rental => rental.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // PAYMENTS
            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.Client)
                .WithMany()
                .HasForeignKey(payment => payment.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.Owner)
                .WithMany()
                .HasForeignKey(payment => payment.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(payment => payment.Request)
                .WithMany()
                .HasForeignKey(payment => payment.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // MESSAGES
            // Base Message relationships to User
            modelBuilder.Entity<Message>()
                .HasOne(message => message.Sender)
                .WithMany()
                .HasForeignKey(message => message.MessageSenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(message => message.Receiver)
                .WithMany()
                .HasForeignKey(message => message.MessageReceiverId)
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

            // CONVERSATION PARTICIPANTS
            modelBuilder.Entity<ConversationParticipant>()
                .HasOne(participant => participant.User)
                .WithMany(user => user.Conversations)
                .HasForeignKey(participant => participant.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // CITIES 
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

            // PRECISIONS
            modelBuilder.Entity<Game>()
                .Property(game => game.PricePerDay)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(payment => payment.PaidAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Rental>()
                .Property(rental => rental.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<User>()
                .Property(user => user.Balance)
                .HasPrecision(18, 2);
        }
    }
}