using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using BoardGames.Shared.DTO;
using BoardGames.Desktop.Services;
using BoardGames.Desktop.Services;
using BoardRentAndProperty.ViewModels;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class RequestsFromOthersViewModelTests
    {
        private readonly Guid sampleOwnerIdentifier = Guid.NewGuid();
        private FakeClientRequestService requestService = null!;
        private FakeCurrentUserContext currentUserContext = null!;
        private RequestsFromOthersViewModel viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            requestService = new FakeClientRequestService
            {
                OpenRequestsForOwner = ImmutableList<RequestDTO>.Empty,
            };
            currentUserContext = new FakeCurrentUserContext { CurrentUserId = sampleOwnerIdentifier };

            viewModel = new RequestsFromOthersViewModel(
                requestService,
                currentUserContext);
        }

        [Test]
        public async Task TryApproveRequest_WhenServiceSucceeds_ReturnsNull()
        {
            requestService.ApproveRequestResult = Result<int, ApproveRequestError>.Success(500);

            string? errorMessage = await viewModel.TryApproveRequestAsync(42);

            Assert.That(errorMessage, Is.Null);
        }

        [Test]
        public async Task TryDenyRequest_WhenServiceReturnsUnauthorized_ReturnsNonNullErrorMessage()
        {
            requestService.DenyRequestResult =
                Result<int, DenyRequestError>.Failure(DenyRequestError.Unauthorized);

            string? errorMessage = await viewModel.TryDenyRequestAsync(42, "unavailable");

            Assert.That(errorMessage, Is.Not.Null);
        }
    }
}
