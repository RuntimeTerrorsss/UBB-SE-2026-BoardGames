using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using BoardGames.Desktop.Services;
using BoardGames.Shared.ProxyServices;
using BoardGames.Shared.DTO;
using BoardGames.Desktop.Services;

namespace BoardGames.Desktop.ViewModels
{
    public class RequestsToOthersViewModel : PagedViewModel<RequestDTO>
    {
        private readonly IRequestService rentalRequestService;
        private readonly ICurrentUserContext currentUserContext;

        public Guid CurrentRenterUserId { get; private set; }

        public RequestsToOthersViewModel(IRequestService rentalRequestService, ICurrentUserContext currentUserContext)
        {
            this.rentalRequestService = rentalRequestService;
            this.currentUserContext = currentUserContext;
            _ = ReloadAsync();
        }

        public override string ShowingText => $"Showing {DisplayedCount} of {TotalCount} requests";

        public Task LoadRequestsAsync() => ReloadAsync();

        protected override void Reload()
        {
            _ = ReloadAsync();
        }

        private async Task ReloadAsync()
        {
            CurrentRenterUserId = currentUserContext.CurrentUserId;
            var requestsResult = await rentalRequestService.GetRequestsForRenterAsync(CurrentRenterUserId);
            var renterRequestsSortedByNewest = requestsResult.Success && requestsResult.Data != null
                ? requestsResult.Data.OrderByDescending(request => request.StartDate).ToImmutableList()
                : ImmutableList<RequestDTO>.Empty;
            SetAllItems(renterRequestsSortedByNewest);
        }

        public async Task<string?> TryCancelRequestAsync(int requestIdToCancel)
        {
            var cancellationAction = new RequestActionDataTransferObject { AccountId = CurrentRenterUserId };
            var cancellationResult = await rentalRequestService.CancelRequestAsync(
                requestIdToCancel,
                cancellationAction);
            if (cancellationResult.Success)
            {
                await ReloadAsync();
                return null;
            }

            return RequestErrorMapper.MapCancel(cancellationResult) switch
            {
                CancelRequestError.NotFound => "Request not found.",
                CancelRequestError.Unauthorized => "You are not authorized to cancel this request.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }

    }
}
