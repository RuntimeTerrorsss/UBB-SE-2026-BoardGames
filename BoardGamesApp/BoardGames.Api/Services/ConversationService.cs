// <copyright file="ConversationService.cs" company="BoardRent">
// Copyright (c) BoardRent. All rights reserved.
// </copyright>

using BoardGames.Data.Models;
using BoardGames.Data.Repositories;
using BoardGames.Shared.DTO;
using static BoardGames.Api.Controllers.ConversationController;

namespace BoardGames.Api.Services
{
    public class ConversationService : IConversationService
    {
        private IConversationRepository ConversationRepository { get; set; }

        private IUserRepository userRepository;
        private IConversationNotifier notifier;

        private int UserId { get; set; }

        private CancellationTokenSource pollingCancellationTokenSource;
        private List<Conversation> cachedConversations = new List<Conversation>();
        private readonly HashSet<int> recentlySentMessageIds = new HashSet<int>();

        public event Action<MessageDTO, string> ActionMessageProcessed;

        public event Action<ConversationDTO, string> ActionConversationProcessed;

        public event Action<ReadReceiptDTO> ActionReadReceiptProcessed;

        public event Action<MessageDTO, string> ActionMessageUpdateProcessed;

        public ConversationService(IConversationRepository conversationRepo, IUserRepository userRepo, IConversationNotifier conversationNotifier)
        {
            this.ConversationRepository = conversationRepo;
            this.userRepository = userRepo;
            this.notifier = conversationNotifier;
        }

        public void Initialize(int userIdInput)
        {
            this.UserId = userIdInput;
            this.notifier.Register(this.UserId, this);
        }

        private async Task NotifySubscribersAboutMessage(Message message)
        {
            IReadOnlyList<int> participants = await this.ConversationRepository.GetParticipantUserIds(message.ConversationId);
            this.notifier.NotifyMessage(participants, message);
        }

        private async Task NotifySubscribersAboutMessageUpdate(Message message)
        {
            IReadOnlyList<int> participants = await this.ConversationRepository.GetParticipantUserIds(message.ConversationId);
            this.notifier.NotifyMessageUpdate(participants, message);
        }

        public async Task<int> FindOrCreateConversationBetweenUsers(int userIdA, int userIdB)
        {
            return await this.ConversationRepository.FindOrCreateConversationBetweenUsers(userIdA, userIdB);
        }

        private async Task NotifySubscribersAboutReadReceipt(ReadReceiptDTO readReceipt)
        {
            IReadOnlyList<int> participants = await this.ConversationRepository.GetParticipantUserIds(readReceipt.ConversationId);
            this.notifier.NotifyReadReceipt(participants, readReceipt);
        }

        private void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            this.notifier.NotifyNewConversation(conversation);
        }

        public async Task<List<ConversationDTO>> FetchConversations()
        {
            List<ConversationDTO> conversationList = new List<ConversationDTO>();
            var systemLookupCache = new Dictionary<int, bool>();

            var fetchedConversations = await this.ConversationRepository.GetConversationsForUser(this.UserId);
            this.cachedConversations = fetchedConversations;

            foreach (var conversation in fetchedConversations)
            {
                ConversationDTO conversationDto = this.ConversationToConversationDTO(conversation);
                bool hasRealOtherParticipant = false;

                foreach (var participant in conversationDto.Participants)
                {
                    if (participant.UserId == this.UserId)
                    {
                        continue;
                    }

                    if (!systemLookupCache.TryGetValue(participant.UserId, out bool isSystemUser))
                    {
                        var user = await this.userRepository.GetById(participant.UserId);
                        isSystemUser = user is not null &&
                                       string.Equals(user.Username, "System", StringComparison.OrdinalIgnoreCase);
                        systemLookupCache[participant.UserId] = isSystemUser;
                    }

                    if (!isSystemUser)
                    {
                        hasRealOtherParticipant = true;
                        break;
                    }
                }

                if (hasRealOtherParticipant)
                {
                    conversationList.Add(conversationDto);
                }
            }

            return conversationList;
        }

        public async Task<string> GetOtherUserNameByConversationDTO(ConversationDTO conversation)
        {
            var otherParticipantIds = conversation.Participants
                .Select(participantItem => participantItem.UserId)
                .Where(participantId => participantId != this.UserId)
                .Distinct()
                .ToList();

            if (otherParticipantIds.Count == 0)
            {
                return "Unknown User";
            }

            foreach (var otherUserId in otherParticipantIds)
            {
                var user = await this.userRepository.GetById(otherUserId);
                if (user is not null &&
                    !string.Equals(user.Username, "System", StringComparison.OrdinalIgnoreCase))
                {
                    return FormatUserDisplayName(user);
                }
            }

            var fallbackUser = await this.userRepository.GetById(otherParticipantIds.First());
            if (fallbackUser is null ||
                string.Equals(fallbackUser.Username, "System", StringComparison.OrdinalIgnoreCase))
            {
                return "Unknown User";
            }

            return FormatUserDisplayName(fallbackUser);
        }

        public string GetOtherUserNameByMessageDTO(MessageDTO message)
        {
            int otherUserId = message.SenderId == this.UserId ? message.ReceiverId : message.SenderId;
            if (otherUserId <= 0)
            {
                return "Unknown User";
            }

            return $"User {otherUserId}";
        }

        public async Task SendMessage(MessageDTO message)
        {
            Message persisted = await this.ConversationRepository.HandleNewMessage(this.MessageDTOToMessage(message));

            // Track the sent ID so the poller won't fire a duplicate notification for it.
            this.recentlySentMessageIds.Add(persisted.MessageId);

            // Immediately update the local cache so the poller sees the message as known.
            var cachedConv = this.cachedConversations.FirstOrDefault(c => c.ConversationId == persisted.ConversationId);
            if (cachedConv != null)
            {
                if (cachedConv.Messages is IList<Message> collection)
                {
                    if (!collection.Any(message => message.MessageId == persisted.MessageId))
                    {
                        collection.Add(persisted);
                    }
                }
            }

            await this.NotifySubscribersAboutMessage(persisted);
        }

        public async Task<int> CreateConversation(int senderId, int receiverId)
        {
            int conversationId = await this.ConversationRepository.CreateConversation(senderId, receiverId);
            Conversation createdConversation = await this.ConversationRepository.GetConversationById(conversationId);
            this.NotifySubscribersAboutNewConversation(createdConversation);
            return conversationId;
        }

        public async Task UpdateMessage(MessageDTO message)
        {
            Message? persisted = await this.ConversationRepository.HandleMessageUpdate(this.MessageDTOToMessage(message));
            if (persisted != null)
            {
                await this.NotifySubscribersAboutMessageUpdate(persisted);
            }
        }

        public async Task SendReadReceipt(ConversationDTO conversation)
        {
            var readReceipt = new ReadReceiptDTO(
                conversation.Id,
                this.UserId,
                conversation.Participants.First(participantItem => participantItem.UserId != this.UserId).UserId,
                DateTime.Now);
            await this.ConversationRepository.HandleReadReceipt(readReceipt);
            await this.NotifySubscribersAboutReadReceipt(readReceipt);
        }

        public async Task OnCardPaymentSelected(int messageId)
        {
            await this.FinalizeRentalRequest(messageId);
        }

        public async Task OnCashPaymentSelected(int messageId, int paymentId)
        {
            await this.FinalizeRentalRequest(messageId);
            await this.SendCashAgreementMessage(messageId, paymentId);
        }

        private async Task FinalizeRentalRequest(int messageId)
        {
            Message? updated = await this.ConversationRepository.HandleRentalRequestFinalization(messageId);
            if (updated != null)
            {
                await this.NotifySubscribersAboutMessageUpdate(updated);
            }
        }

        private async Task SendCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            Message? created = await this.ConversationRepository.CreateCashAgreementMessage(messageIdOfParentRentalRequestMessage, paymentId);
            if (created != null)
            {
                await this.NotifySubscribersAboutMessage(created);
            }
        }

        public void StartPolling()
        {
            if (this.pollingCancellationTokenSource != null)
            {
                return;
            }

            this.pollingCancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(() => this.PollConversationsLoop(this.pollingCancellationTokenSource.Token));
        }

        public void StopPolling()
        {
            this.pollingCancellationTokenSource?.Cancel();
            this.pollingCancellationTokenSource?.Dispose();
            this.pollingCancellationTokenSource = null;
        }

        private async Task PollConversationsLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(0.1));
                    var fetchedConversations = await this.ConversationRepository.GetConversationsForUser(this.UserId);

                    foreach (var fetchedConv in fetchedConversations)
                    {
                        var cachedConv = this.cachedConversations.FirstOrDefault(c => c.ConversationId == fetchedConv.ConversationId);

                        if (cachedConv == null)
                        {
                            this.NotifySubscribersAboutNewConversation(fetchedConv);
                        }
                        else
                        {
                            foreach (var fetchedMsg in fetchedConv.Messages)
                            {
                                var cachedMsg = cachedConv.Messages.FirstOrDefault(message => message.MessageId == fetchedMsg.MessageId);
                                if (cachedMsg == null)
                                {
                                    // Only notify if we didn't just send this message ourselves.
                                    if (!this.recentlySentMessageIds.Remove(fetchedMsg.MessageId))
                                    {
                                        await this.NotifySubscribersAboutMessage(fetchedMsg);
                                    }
                                }
                                else
                                {
                                    bool updated = false;
                                    if (fetchedMsg is RentalRequestMessage fetchedRental && cachedMsg is RentalRequestMessage cachedRental)
                                    {
                                        if (fetchedRental.IsRequestResolved != cachedRental.IsRequestResolved ||
                                            fetchedRental.IsRequestAccepted != cachedRental.IsRequestAccepted)
                                        {
                                            updated = true;
                                        }
                                    }
                                    else if (fetchedMsg is CashAgreementMessage fetchedCash && cachedMsg is CashAgreementMessage cachedCash)
                                    {
                                        if (fetchedCash.IsCashAgreementResolved != cachedCash.IsCashAgreementResolved ||
                                            fetchedCash.IsCashAgreementAcceptedByBuyer != cachedCash.IsCashAgreementAcceptedByBuyer ||
                                            fetchedCash.IsCashAgreementAcceptedBySeller != cachedCash.IsCashAgreementAcceptedBySeller)
                                        {
                                            updated = true;
                                        }
                                    }

                                    if (updated)
                                    {
                                        await this.NotifySubscribersAboutMessageUpdate(fetchedMsg);
                                    }
                                }
                            }
                        }
                    }

                    this.cachedConversations = fetchedConversations;
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                }
            }
        }

        public void OnMessageReceived(Message message)
        {
            MessageDTO messageDTO = this.MessageToMessageDTO(message);
            string userName = this.GetOtherUserNameByMessageDTO(messageDTO);
            this.ActionMessageProcessed?.Invoke(messageDTO, userName);
        }

        public async Task OnConversationReceived(Conversation conversation)
        {
            ConversationDTO conversationDTO = this.ConversationToConversationDTO(conversation);
            string userName = await this.GetOtherUserNameByConversationDTO(conversationDTO);
            this.ActionConversationProcessed?.Invoke(conversationDTO, userName);
        }

        public void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
        {
            this.ActionReadReceiptProcessed?.Invoke(readReceipt);
        }

        public void OnMessageUpdateReceived(Message message)
        {
            MessageDTO messageDTO = this.MessageToMessageDTO(message);
            string userName = this.GetOtherUserNameByMessageDTO(messageDTO);
            this.ActionMessageUpdateProcessed?.Invoke(messageDTO, userName);
        }

        public Message MessageDTOToMessage(MessageDTO messageDto)
        {
            Message toReturn = messageDto.Type switch
            {
                MessageType.MessageText => new TextMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    TextMessageContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageImage => new ImageMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    MessageImageUrl = messageDto.ImageUrl,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageRentalRequest => new RentalRequestMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    RentalRequestId = messageDto.RequestId,
                    IsRequestResolved = messageDto.IsResolved,
                    IsRequestAccepted = messageDto.IsAccepted,
                    RequestContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageCashAgreement => new CashAgreementMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    CashPaymentId = messageDto.PaymentId,
                    IsCashAgreementResolved = messageDto.IsResolved,
                    IsCashAgreementAcceptedByBuyer = messageDto.IsAcceptedByBuyer,
                    IsCashAgreementAcceptedBySeller = messageDto.IsAcceptedBySeller,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageSystem => new SystemMessage
                {
                    MessageId = messageDto.Id,
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    MessageContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(messageDto.Type), messageDto.Type, "Unsupported message type."),
            };

            return toReturn;
        }

        public MessageDTO MessageToMessageDTO(Message message)
        {
            int defaultMissingIdentifier = -1;

            MessageType messageType = message switch
            {
                TextMessage => MessageType.MessageText,
                ImageMessage => MessageType.MessageImage,
                RentalRequestMessage => MessageType.MessageRentalRequest,
                CashAgreementMessage => MessageType.MessageCashAgreement,
                SystemMessage => MessageType.MessageSystem,
                _ => throw new ArgumentOutOfRangeException(nameof(message), message.GetType().Name, "Unknown message subtype."),
            };

            string content = message switch
            {
                TextMessage textMessage => textMessage.TextMessageContent ?? textMessage.MessageContentAsString ?? string.Empty,
                RentalRequestMessage rentalForContent => rentalForContent.RequestContent ?? rentalForContent.MessageContentAsString ?? string.Empty,
                SystemMessage systemMessage => systemMessage.MessageContent ?? systemMessage.MessageContentAsString ?? string.Empty,
                _ => message.MessageContentAsString ?? string.Empty,
            };

            return new MessageDTO(
                Id: message.MessageId,
                ConversationId: message.ConversationId,
                SenderId: message.MessageSenderId,
                ReceiverId: message.MessageReceiverId,
                SentAt: message.MessageSentTime,
                Content: content,
                Type: messageType,
                ImageUrl: message is ImageMessage imageMessage ? imageMessage.MessageImageUrl ?? string.Empty : string.Empty,
                IsResolved: message is RentalRequestMessage rentalResolvedMessage ? rentalResolvedMessage.IsRequestResolved
                          : message is CashAgreementMessage cashResolvedMessage ? cashResolvedMessage.IsCashAgreementResolved
                          : false,
                IsAccepted: message is RentalRequestMessage rentalAcceptedMessage ? rentalAcceptedMessage.IsRequestAccepted : false,
                IsAcceptedByBuyer: message is CashAgreementMessage cashBuyerMessage ? cashBuyerMessage.IsCashAgreementAcceptedByBuyer : false,
                IsAcceptedBySeller: message is CashAgreementMessage cashSellerMessage ? cashSellerMessage.IsCashAgreementAcceptedBySeller : false,
                PaymentId: message is CashAgreementMessage cashPaymentMessage ? cashPaymentMessage.CashPaymentId : defaultMissingIdentifier,
                RequestId: message is RentalRequestMessage rentalRequestMessage ? rentalRequestMessage.RentalRequestId : defaultMissingIdentifier);
        }

        public ConversationDTO ConversationToConversationDTO(Conversation conversation)
        {
            var messageDTOs = conversation.Messages
                .OrderBy(messageItem => messageItem.MessageSentTime)
                .Select(messageItem => this.MessageToMessageDTO(messageItem))
                .ToList();

            var participantsOrdered = conversation.Participants
                .OrderBy(participantItem => participantItem.UserId)
                .Select(participantItem => new ConversationParticipantDTO
                {
                    ConversationId = participantItem.ConversationId,
                    UserId = participantItem.UserId,
                    LastMessageReadTime = participantItem.LastMessageReadTime,
                    UnreadMessagesCount = participantItem.UnreadMessagesCount,
                })
                .ToList();

            var lastRead = conversation.Participants.ToDictionary(
                participantItem => participantItem.UserId,
                participantItem => participantItem.LastMessageReadTime ?? DateTime.MinValue);

            return new ConversationDTO(
                conversationId: conversation.ConversationId,
                participants: participantsOrdered,
                messages: messageDTOs,
                lastRead: lastRead);
        }

        private static string FormatUserDisplayName(User user)
        {
            return !string.IsNullOrWhiteSpace(user.DisplayName) ? user.DisplayName : user.Username;
        }
    }
}
