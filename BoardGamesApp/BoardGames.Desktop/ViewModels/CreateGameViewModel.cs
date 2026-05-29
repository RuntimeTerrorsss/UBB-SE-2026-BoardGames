
using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using AppConstants = BoardGames.Desktop.Constants.Constants;
using DomainValues = BoardGames.Desktop.Constants.DomainConstants;

namespace BoardGames.Desktop.ViewModels
{
    public class CreateGameViewModel
    {
        private const int NoValidationErrors = 0;
        private const int NewGameId = 0;
        private const decimal ZeroPriceForEmptyOrInvalidInput = 0m;

        private readonly IGameService gameListingService;
        private readonly ICurrentUserContext currentUserContext;

        public string GameName { get; set; } = string.Empty;

        public decimal GamePrice { get; set; }

        public double GamePriceAsDouble
        {
            get => (double)GamePrice;
            set => GamePrice = (decimal)value;
        }

        public int MinimumPlayersRequired { get; set; } = DomainValues.GameDefaultMinimumPlayers;

        public int MaximumPlayersAllowed { get; set; } = DomainValues.GameDefaultMaximumPlayers;

        public string GameDescription { get; set; } = string.Empty;

        public bool IsGameActive { get; set; } = true;

        public byte[]? GameImage { get; set; }

        public Guid CurrentUserId => currentUserContext.CurrentUserId;

        public CreateGameViewModel(IGameService gameListingService, ICurrentUserContext currentUserContext)
        {
            this.gameListingService = gameListingService;
            this.currentUserContext = currentUserContext;
        }

        public List<string> ValidateGameInputs()
        {
            return GameInputValidator.Validate(BuildValidationGameDTO());
        }

        public async Task<ViewOperationResult> SubmitCreateGameAsync()
        {
            var gameValidationErrors = ValidateGameInputs();
            if (gameValidationErrors.Count > NoValidationErrors)
            {
                return ViewOperationResult.Failure(
                    AppConstants.DialogTitles.ValidationError,
                    string.Join(Environment.NewLine, gameValidationErrors));
            }

            var saveResult = await SaveGameAsync();
            return saveResult;
        }

        public void SetGamePriceFromText(string rawPriceText)
        {
            if (PriceInputParser.TryParsePriceInput(rawPriceText, out var parsedPriceAsDouble))
            {
                GamePriceAsDouble = parsedPriceAsDouble;
                return;
            }

            GamePrice = ZeroPriceForEmptyOrInvalidInput;
        }

        public async Task<ViewOperationResult> SaveGameAsync()
        {
            if (GameInputValidator.Validate(BuildValidationGameDTO()).Count > NoValidationErrors)
            {
                return ViewOperationResult.Failure(
                    AppConstants.DialogTitles.ValidationError,
                    AppConstants.DialogMessages.UnexpectedErrorOccurred);
            }

            var createDto = BuildGameCreateDTO();
            var createGameResult = await gameListingService.CreateGameAsync(createDto);

            return createGameResult.Success
                ? ViewOperationResult.Success()
                : ViewOperationResult.Failure(
                    AppConstants.DialogTitles.ValidationError,
                    createGameResult.Error ?? AppConstants.DialogMessages.UnexpectedErrorOccurred);
        }

        private GameSummaryDTO BuildGameSummaryDTO()
        {
            return new GameSummaryDTO
            {
                Id = NewGameId,
                OwnerAccountId = CurrentUserId,
                Name = GameName,
                Price = GamePrice,
                MinimumPlayerNumber = MinimumPlayersRequired,
                MaximumPlayerNumber = MaximumPlayersAllowed,
                IsActive = IsGameActive
            };
        }

        private GameCreateDTO BuildGameCreateDTO()
        {
            return new GameCreateDTO
            {
                OwnerAccountId = CurrentUserId,
                Name = GameName,
                Price = GamePrice,
                MinimumPlayerNumber = MinimumPlayersRequired,
                MaximumPlayerNumber = MaximumPlayersAllowed,
                Description = GameDescription,
                Image = GameImage ?? Array.Empty<byte>(),
            };
        }

        private GameDTO BuildValidationGameDTO()
        {
            return new GameDTO
            {
                Id = NewGameId,
                Owner = new UserDTO { Id = CurrentUserId },
                Name = GameName,
                Price = GamePrice,
                MinimumPlayerNumber = MinimumPlayersRequired,
                MaximumPlayerNumber = MaximumPlayersAllowed,
                Description = GameDescription,
                Image = GameImage,
                IsActive = IsGameActive
            };
        }
    }
}
