// <copyright file="IProxyServices.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Shared.DTO;
using BoardGames.Web.Models.Account;

namespace BoardGames.Web.Infrastructure
{
    public interface IAuthProxyService
    {
        Task<AccountProfileDTO> LoginAsync(LoginDTO body, CancellationToken cancellationToken = default);

        Task RegisterAsync(RegisterDTO body, CancellationToken cancellationToken = default);

        Task LogoutAsync(CancellationToken cancellationToken = default);

        Task<string> ForgotPasswordAsync(CancellationToken cancellationToken = default);
    }

    public interface IAccountProxyService
    {
        Task<AccountProfileDTO> GetProfileAsync(Guid accountId);

        Task UpdateProfileAsync(Guid accountId, AccountProfileDTO updateData);

        Task UploadAvatarAsync(Guid accountId, string imagePath);

        Task RemoveAvatarAsync(Guid accountId);

        Task ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword);
    }

    public interface IAdminProxyService
    {
        Task<IEnumerable<AdminAccountViewModel>> GetAllAccountsAsync();

        Task SuspendAccountAsync(string accountId);

        Task UnsuspendAccountAsync(string accountId);

        Task UnlockAccountAsync(string accountId);

        Task ResetPasswordAsync(string accountId, string newPassword);
    }

    public interface IGameProxyService
    {
        Task<IReadOnlyList<GameDTO>> GetAllGamesAsync(CancellationToken cancellationToken = default);

        Task<GameDTO?> GetGameByIdAsync(int gameId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GameDTO>> GetGamesByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GameDTO>> GetAvailableGamesForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<GameDTO>> SearchGamesAsync(GameSearchCriteriaDTO criteria, CancellationToken cancellationToken = default);

        Task CreateGameAsync(GameDTO body, CancellationToken cancellationToken = default);

        Task UpdateGameAsync(int gameId, GameDTO body, CancellationToken cancellationToken = default);

        Task DeleteGameAsync(int gameId, CancellationToken cancellationToken = default);
    }

    public interface IRentalProxyService
    {
        Task<IReadOnlyList<RentalDTO>> GetRentalsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RentalDTO>> GetRentalsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<BookedDateRangeDTO>> GetBookedDatesForGameAsync(int gameId, CancellationToken cancellationToken = default);

        Task<bool> CheckAvailabilityAsync(int gameId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }

    public interface IRequestProxyService
    {
        Task<IReadOnlyList<RequestDTO>> GetOpenRequestsForOwnerAsync(Guid ownerAccountId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<RequestDTO>> GetRequestsForRenterAsync(Guid renterAccountId, CancellationToken cancellationToken = default);

        Task CreateRequestAsync(CreateRequestDTO body, CancellationToken cancellationToken = default);

        Task OfferGameAsync(int requestId, RequestActionDTO body, CancellationToken cancellationToken = default);

        Task DenyRequestAsync(int requestId, RequestActionDTO body, CancellationToken cancellationToken = default);

        Task CancelRequestAsync(int requestId, RequestActionDTO body, CancellationToken cancellationToken = default);
    }

    public interface INotificationProxyService
    {
        Task<IReadOnlyList<NotificationDTO>> GetNotificationsForUserAsync(Guid accountId);

        Task DeleteNotificationAsync(int notificationId);
    }

    public interface IConversationProxyService
    {
        Task<IReadOnlyList<ConversationDTO>> GetConversationsForUserAsync(Guid accountId, CancellationToken cancellationToken = default);

        Task<ConversationDTO?> GetConversationByIdAsync(int conversationId, CancellationToken cancellationToken = default);

        Task<int> FindOrCreateConversationAsync(Guid senderAccountId, Guid receiverAccountId, CancellationToken cancellationToken = default);

        Task<MessageDataTransferObject> SendMessageAsync(MessageDataTransferObject message, CancellationToken cancellationToken = default);

        Task<MessageDataTransferObject> UpdateMessageAsync(MessageDataTransferObject message, CancellationToken cancellationToken = default);
    }

    public interface IChatProxyService : IConversationProxyService
    {
    }
}
