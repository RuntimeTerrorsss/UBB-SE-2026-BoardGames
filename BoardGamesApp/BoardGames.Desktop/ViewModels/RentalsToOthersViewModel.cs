using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;
using BoardGames.Desktop.Services;

namespace BoardGames.Desktop.ViewModels
{
    public class RentalsToOthersViewModel : PagedViewModel<RentalDTO>
    {
        private readonly IRentalService rentalLookupService;
        private readonly ICurrentUserContext currentUserContext;

        public Guid CurrentGameOwnerUserId { get; private set; }

        public RentalsToOthersViewModel(IRentalService rentalLookupService, ICurrentUserContext currentUserContext)
        {
            this.rentalLookupService = rentalLookupService;
            this.currentUserContext = currentUserContext;
            _ = ReloadAsync();
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} rentals";

        public Task LoadRentalsAsync() => ReloadAsync();

        protected override void Reload()
        {
            _ = ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            CurrentGameOwnerUserId = currentUserContext.CurrentUserId;
            var rentalsResult = await rentalLookupService.GetRentalsForOwnerAsync(CurrentGameOwnerUserId);
            var ownerRentalsSortedByNewest = rentalsResult.Success && rentalsResult.Data != null
                ? rentalsResult.Data.OrderByDescending(rental => rental.StartDate).ToImmutableList()
                : ImmutableList<RentalDTO>.Empty;
            SetAllItems(ownerRentalsSortedByNewest);
        }
    }
}
