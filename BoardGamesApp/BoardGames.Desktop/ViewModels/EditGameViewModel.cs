using BoardGames.Desktop.Services;
using BoardGames.Shared.DTO;
using BoardGames.Shared.ProxyServices;
using AppConstants = BoardGames.Desktop.Constants.Constants;
using DomainValues = BoardGames.Desktop.Constants.DomainConstants;

namespace BoardGames.Desktop.ViewModels
{
    public class EditGameViewModel
    {
        private readonly IGameService gameService;
        private readonly IDesktopAuthorizationService authorizationService;
        private int currentGameId;

        public EditGameViewModel(IGameService gameService, IDesktopAuthorizationService authorizationService)
        {
            this.gameService = gameService;
            this.authorizationService = authorizationService;
        }

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

        public bool HasGameImage => GameImage is { Length: > 0 };

        public async Task LoadGameAsync(int gameId)
        {
            currentGameId = gameId;
            var result = await gameService.GetGameDetailsByIdAsync(gameId);
            if (!result.Success || result.Data == null)
            {
                throw new InvalidOperationException(result.Error ?? "Game could not be loaded.");
            }

            var game = result.Data;
            if (!authorizationService.IsAdministrator && game.OwnerAccountId != authorizationService.CurrentAccountId)
            {
                throw new UnauthorizedAccessException("You are not authorized to edit this game.");
            }

            GameName = game.Name;
            GamePrice = game.Price;
            MinimumPlayersRequired = game.MinimumPlayerNumber;
            MaximumPlayersAllowed = game.MaximumPlayerNumber;
            GameDescription = game.Description;
            IsGameActive = game.IsActive;
        }

        public void SetGamePriceFromText(string rawPriceText)
        {
            if (PriceInputParser.TryParsePriceInput(rawPriceText, out var parsedPriceAsDouble))
            {
                GamePriceAsDouble = parsedPriceAsDouble;
            }
        }

        public async Task<ViewOperationResult> SubmitGameUpdateAsync()
        {
            var validationErrors = GameInputValidator.Validate(BuildValidationGameDTO());
            if (validationErrors.Count > 0)
            {
                return ViewOperationResult.Failure(
                    AppConstants.DialogTitles.ValidationError,
                    string.Join(Environment.NewLine, validationErrors));
            }

            var updateResult = await gameService.UpdateGameAsync(currentGameId, BuildGameSummaryDTO());
            return updateResult.Success
                ? ViewOperationResult.Success()
                : ViewOperationResult.Failure(
                    AppConstants.DialogTitles.RequestFailed,
                    updateResult.Error ?? AppConstants.DialogMessages.UnexpectedErrorOccurred);
        }

        private GameSummaryDTO BuildGameSummaryDTO()
        {
            return new GameSummaryDTO
            {
                Id = currentGameId,
                OwnerAccountId = authorizationService.CurrentAccountId,
                Name = GameName,
                Price = GamePrice,
                MinimumPlayerNumber = MinimumPlayersRequired,
                MaximumPlayerNumber = MaximumPlayersAllowed,
                IsActive = IsGameActive
            };
        }

        private GameDTO BuildValidationGameDTO()
        {
            return new GameDTO
            {
                Id = currentGameId,
                Owner = new UserDTO { Id = authorizationService.CurrentAccountId },
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
