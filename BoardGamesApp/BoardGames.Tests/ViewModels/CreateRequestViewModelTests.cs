using BoardGames.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using BoardGames.Tests.Fakes;
using NUnit.Framework;

namespace BoardGames.Tests.ViewModels
{
    [TestFixture]
    public sealed class CreateRequestViewModelTests
    {
        private readonly Guid currentUserId = Guid.NewGuid();
        private readonly Guid otherOwnerId = Guid.NewGuid();

        private FakeClientGameService gameService = null!;
        private FakeClientRequestService requestService = null!;
        private FakeCurrentUserContext currentUserContext = null!;

        [SetUp]
        public void SetUp()
        {
            gameService = new FakeClientGameService
            {
                AvailableGamesForRenter = ImmutableList.Create(BuildOtherUsersGame(300)),
            };
            requestService = new FakeClientRequestService();
            currentUserContext = new FakeCurrentUserContext { CurrentUserId = currentUserId };
        }

        [Test]
        public async Task Constructor_LoadsGamesCurrentUserAndRefreshesCollection()
        {
            var viewModel = BuildViewModel();

            Assert.Multiple(() =>
            {
                Assert.That(viewModel.CurrentUserId, Is.EqualTo(currentUserId));
                Assert.That(viewModel.AvailableGamesToRequest.Count, Is.EqualTo(1));
                Assert.That(viewModel.AvailableGamesToRequest[0].Id, Is.EqualTo(300));
                Assert.That(viewModel.AvailableGamesToRequest[0].Owner.Id, Is.Not.EqualTo(currentUserId));
                Assert.That(viewModel.AvailableGamesToRequest[0].IsActive, Is.True);
            });

            gameService.AvailableGamesForRenter =
                ImmutableList.Create(BuildOtherUsersGame(300), BuildOtherUsersGame(401));

            await viewModel.LoadAvailableGamesAsync();

            Assert.That(viewModel.AvailableGamesToRequest.Count, Is.EqualTo(2));
        }

        [Test]
        public void ValidateRequestInputs_RequiresGameAndDates()
        {
            var viewModel = BuildViewModel();

            PopulateWithValidSelections(viewModel);
            Assert.That(viewModel.ValidateRequestInputs(), Is.True);

            AssertInvalidRequestInputs(viewModel, model => model.SelectedGame = null!);
            AssertInvalidRequestInputs(viewModel, model => model.StartDate = null);
            AssertInvalidRequestInputs(viewModel, model => model.EndDate = null);
        }

        [Test]
        public async Task SubmitRequest_CoversValidationSuccessAndInvalidDateRange()
        {
            var invalidViewModel = BuildViewModel();

            ViewOperationResult validationFailure = await invalidViewModel.SubmitRequestAsync();

            Assert.Multiple(() =>
            {
                Assert.That(validationFailure.IsSuccess, Is.False);
                Assert.That(validationFailure.DialogTitle, Is.EqualTo("Validation Error"));
            });
            Assert.That(requestService.CreateRequestCallCount, Is.EqualTo(0));

            requestService.CreateRequestResult = Result<int, CreateRequestError>.Success(1);

            var successfulViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulViewModel);

            ViewOperationResult successResult = await successfulViewModel.SubmitRequestAsync();

            Assert.That(successResult.IsSuccess, Is.True);
            Assert.That(requestService.CreateRequestCallCount, Is.EqualTo(1));
            Assert.That(requestService.LastGameId, Is.EqualTo(300));
            Assert.That(requestService.LastRenterAccountId, Is.EqualTo(currentUserId));
            Assert.That(requestService.LastOwnerAccountId, Is.EqualTo(otherOwnerId));

            requestService.CreateRequestResult =
                Result<int, CreateRequestError>.Failure(CreateRequestError.InvalidDateRange);

            var invalidDateRangeViewModel = BuildViewModel();
            PopulateWithValidSelections(invalidDateRangeViewModel);

            ViewOperationResult invalidDateRangeResult = await invalidDateRangeViewModel.SubmitRequestAsync();

            Assert.Multiple(() =>
            {
                Assert.That(invalidDateRangeResult.IsSuccess, Is.False);
                Assert.That(invalidDateRangeResult.DialogTitle, Is.EqualTo("Validation Error"));
            });
        }

        [Test]
        public async Task SubmitRequest_MapsServiceErrorsAndTrySubmitRequestMirrorsResult()
        {
            requestService.CreateRequestResult =
                Result<int, CreateRequestError>.Failure(CreateRequestError.OwnerCannotRent);

            var ownerCannotRentViewModel = BuildViewModel();
            PopulateWithValidSelections(ownerCannotRentViewModel);

            ViewOperationResult ownerCannotRentResult = await ownerCannotRentViewModel.SubmitRequestAsync();
            string? ownerCannotRentTrySubmitMessage = await ownerCannotRentViewModel.TrySubmitRequestAsync();

            Assert.Multiple(() =>
            {
                Assert.That(ownerCannotRentResult.IsSuccess, Is.False);
                Assert.That(ownerCannotRentResult.DialogTitle, Is.EqualTo("Request Failed"));
                Assert.That(ownerCannotRentResult.DialogMessage, Does.Contain("own game"));
                Assert.That(ownerCannotRentTrySubmitMessage, Does.Contain("own game"));
            });

            requestService.CreateRequestResult =
                Result<int, CreateRequestError>.Failure(CreateRequestError.DatesUnavailable);

            var datesUnavailableViewModel = BuildViewModel();
            PopulateWithValidSelections(datesUnavailableViewModel);
            ViewOperationResult datesUnavailableResult = await datesUnavailableViewModel.SubmitRequestAsync();
            Assert.That(datesUnavailableResult.DialogMessage, Does.Contain("not available"));

            requestService.CreateRequestResult =
                Result<int, CreateRequestError>.Failure(CreateRequestError.GameDoesNotExist);

            var missingGameViewModel = BuildViewModel();
            PopulateWithValidSelections(missingGameViewModel);
            ViewOperationResult missingGameResult = await missingGameViewModel.SubmitRequestAsync();
            Assert.That(missingGameResult.DialogMessage, Does.Contain("no longer exists"));

            requestService.CreateRequestResult = Result<int, CreateRequestError>.Success(1);

            var successfulTrySubmitViewModel = BuildViewModel();
            PopulateWithValidSelections(successfulTrySubmitViewModel);
            string? successfulTrySubmitMessage = await successfulTrySubmitViewModel.TrySubmitRequestAsync();
            Assert.That(successfulTrySubmitMessage, Is.Null);
        }

        [Test]
        public void Setters_RaisePropertyChangedForBindableFields()
        {
            var viewModel = BuildViewModel();
            var changedProperties = new List<string?>();
            viewModel.PropertyChanged += (_, eventArgs) => changedProperties.Add(eventArgs.PropertyName);

            viewModel.SelectedGame = BuildOtherUsersGame(888);
            viewModel.StartDate = DateTimeOffset.Now.AddDays(2);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(10);

            Assert.That(changedProperties, Is.EqualTo(new[]
            {
                nameof(viewModel.SelectedGame),
                nameof(viewModel.StartDate),
                nameof(viewModel.EndDate),
            }));
        }

        private CreateRequestViewModel BuildViewModel()
        {
            return new CreateRequestViewModel(
                gameService,
                requestService,
                currentUserContext);
        }

        private static void AssertInvalidRequestInputs(CreateRequestViewModel viewModel, Action<CreateRequestViewModel> invalidate)
        {
            PopulateWithValidSelections(viewModel);
            invalidate(viewModel);
            Assert.That(viewModel.ValidateRequestInputs(), Is.False);
        }

        private static void PopulateWithValidSelections(CreateRequestViewModel viewModel)
        {
            viewModel.SelectedGame = viewModel.AvailableGamesToRequest[0];
            viewModel.StartDate = DateTimeOffset.Now.AddDays(1);
            viewModel.EndDate = DateTimeOffset.Now.AddDays(7);
        }

        private GameDTO BuildOtherUsersGame(int gameId)
        {
            return new GameDTO
            {
                Id = gameId,
                Owner = new UserDTO { Id = otherOwnerId },
                Name = $"Board Game {gameId}",
                Price = 12m,
                IsActive = true,
            };
        }
    }
}
