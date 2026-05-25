using BoardGames.Desktop.Services;

namespace BoardGames.Desktop.ViewModels
{
    public class RequestsFromOthersViewModel : PagedViewModel<RequestDTO>
    {
        private readonly IRequestService rentalRequestService;
        private readonly ICurrentUserContext currentUserContext;

        public Guid CurrentGameOwnerUserId { get; private set; }

        public RequestsFromOthersViewModel(IRequestService rentalRequestService, ICurrentUserContext currentUserContext)
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
            CurrentGameOwnerUserId = currentUserContext.CurrentUserId;

            var requestsResult = await rentalRequestService.GetOpenRequestsForOwnerAsync(CurrentGameOwnerUserId);
            var openRequestsForOwnerSortedByNewest = requestsResult.Success && requestsResult.Data != null
                ? requestsResult.Data.OrderByDescending(request => request.StartDate).ToImmutableList()
                : ImmutableList<RequestDTO>.Empty;
            SetAllItems(openRequestsForOwnerSortedByNewest);
        }

        public async Task<string?> TryApproveRequestAsync(int requestIdToApprove)
        {
            var approvalResult = await rentalRequestService.ApproveRequestAsync(
                requestIdToApprove,
                CurrentGameOwnerUserId);
            if (approvalResult.Success)
            {
                await ReloadAsync();
                return null;
            }

            return RequestErrorMapper.MapApprove(approvalResult) switch
            {
                ApproveRequestError.Unauthorized => "You are not authorized to approve this request.",
                ApproveRequestError.NotFound => "Request not found.",
                ApproveRequestError.TransactionFailed => "Could not approve the request. Please try again.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }

        public async Task<string?> TryDenyRequestAsync(int requestIdToDeny, string? rawDenialReason)
        {
            var denialAction = new RequestActionDataTransferObject
            {
                AccountId = CurrentGameOwnerUserId,
                Reason = rawDenialReason ?? string.Empty,
            };

            var denialResult = await rentalRequestService.DenyRequestAsync(requestIdToDeny, denialAction);
            if (denialResult.Success)
            {
                await ReloadAsync();
                return null;
            }

            return RequestErrorMapper.MapDeny(denialResult) switch
            {
                DenyRequestError.NotFound => "Request not found.",
                DenyRequestError.Unauthorized => "You are not authorized to deny this request.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }

        public async Task<string?> TryOfferGameAsync(int requestIdForGameOffer)
        {
            var offerAction = new RequestActionDataTransferObject { AccountId = CurrentGameOwnerUserId };
            var gameOfferResult = await rentalRequestService.OfferGameAsync(
                requestIdForGameOffer,
                offerAction);
            if (gameOfferResult.Success)
            {
                await ReloadAsync();
                return null;
            }

            return RequestErrorMapper.MapOffer(gameOfferResult) switch
            {
                OfferError.NotFound => "Request not found.",
                OfferError.NotOwner => "You are not the owner of this game.",
                OfferError.RequestNotOpen => "This request is no longer open.",
                OfferError.TransactionFailed => "Could not approve the request. Please try again.",
                _ => Constants.DialogMessages.UnexpectedErrorOccurred
            };
        }

    }
}
