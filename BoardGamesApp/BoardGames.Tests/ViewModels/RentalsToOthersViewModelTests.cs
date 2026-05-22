using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardRentAndProperty.Contracts.DataTransferObjects;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class RentalsToOthersViewModelTests
    {
        private readonly Guid ownerIdentifier = Guid.NewGuid();
        private readonly Guid renterIdentifier = Guid.NewGuid();
        private FakeClientRentalService rentalService = null!;
        private FakeCurrentUserContext currentUserContext = null!;

        [SetUp]
        public void SetUp()
        {
            rentalService = new FakeClientRentalService();
            currentUserContext = new FakeCurrentUserContext { CurrentUserId = ownerIdentifier };
        }

        [Test]
        public void ShowingText_WithRentals_ContainsCountAndRentalsKeyword()
        {
            rentalService.RentalsForOwner =
                ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30), BuildRental(50));

            var viewModel = new RentalsToOthersViewModel(rentalService, currentUserContext);

            Assert.That(viewModel.ShowingText, Does.Contain("rentals"));
            Assert.That(viewModel.ShowingText, Does.Contain("4"));
        }

        [Test]
        public void Constructor_WithRentals_SetsCorrectOwnerIdAndTotalCount()
        {
            rentalService.RentalsForOwner =
                ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30), BuildRental(40));

            var viewModel = new RentalsToOthersViewModel(rentalService, currentUserContext);

            Assert.That(viewModel.TotalCount, Is.EqualTo(4));
            Assert.That(viewModel.CurrentGameOwnerUserId, Is.EqualTo(ownerIdentifier));
        }

        [Test]
        public void Constructor_WithRentals_PagedItemsContainCorrectRentalDetails()
        {
            rentalService.RentalsForOwner = ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30));

            var viewModel = new RentalsToOthersViewModel(rentalService, currentUserContext);
            var pagedRentalIds = viewModel.PagedItems.Select(rental => rental.Id).ToList();

            Assert.That(viewModel.PagedItems.All(rental => rental.Game.Id == 1), Is.True);
            Assert.That(viewModel.PagedItems.All(rental => rental.Owner.Id == ownerIdentifier), Is.True);
            Assert.That(pagedRentalIds, Does.Contain(10));
            Assert.That(pagedRentalIds, Does.Contain(20));
            Assert.That(pagedRentalIds, Does.Contain(30));
        }

        [Test]
        public async Task LoadRentals_AfterServiceDataChanged_RefreshesTotalCountAndPagedItems()
        {
            rentalService.RentalsForOwner = ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30));

            var viewModel = new RentalsToOthersViewModel(rentalService, currentUserContext);
            Assert.That(viewModel.TotalCount, Is.EqualTo(3));

            rentalService.RentalsForOwner =
                ImmutableList.Create(BuildRental(10), BuildRental(20), BuildRental(30), BuildRental(50));

            await viewModel.LoadRentalsAsync();

            var pagedRentalIds = viewModel.PagedItems.Select(rental => rental.Id).ToList();

            Assert.That(viewModel.TotalCount, Is.EqualTo(4));
            Assert.That(pagedRentalIds, Does.Contain(50));
        }

        private RentalDTO BuildRental(int rentalId)
        {
            return new RentalDTO
            {
                Id = rentalId,
                Game = new GameDTO { Id = 1 },
                Renter = new UserDTO { Id = renterIdentifier },
                Owner = new UserDTO { Id = ownerIdentifier },
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
            };
        }
    }
}
