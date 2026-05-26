using BoardGames.Data.Models;
using BoardGames.Shared.DTO;
// <copyright file="ConversationService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using BoardGames.Data.Enums;
using BoardGames.Data.Repositories;

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

        public event Action<MessageDataTransferObject, string> ActionMessageProcessed;

        public event Action<ConversationDTO, string> ActionConversationProcessed;

        public event Action<ReadReceiptDTO> ActionReadReceiptProcessed;

        public event Action<MessageDataTransferObject, string> ActionMessageUpdateProcessed;

        public ConversationService(IConversationRepository conversationRepo, IUserRepository userRepo, IConversationNotifier conversationNotifier)
        {
            ConversationRepository = conversationRepo;
            userRepository = userRepo;
            notifier = conversationNotifier;
        }

        public void Initialize(int userIdInput)
        {
            UserId = userIdInput;
            notifier.Register(UserId, this);
        }
        private async Task NotifySubscribersAboutMessage(Message message)
        {
            IReadOnlyList<int> participants = await ConversationRepository.GetParticipantUserIds(message.ConversationId);
            notifier.NotifyMessage(participants, message);
        }

        private async Task NotifySubscribersAboutMessageUpdate(Message message)
        {
            IReadOnlyList<int> participants = await ConversationRepository.GetParticipantUserIds(message.ConversationId);
            notifier.NotifyMessageUpdate(participants, message);
        }

        public async Task<int> FindOrCreateConversationBetweenUsers(int userIdA, int userIdB)
        {
            return await ConversationRepository.FindOrCreateConversationBetweenUsers(userIdA, userIdB);
        }

        private async Task NotifySubscribersAboutReadReceipt(ReadReceiptDTO readReceipt)
        {
            IReadOnlyList<int> participants = await ConversationRepository.GetParticipantUserIds(readReceipt.ConversationId);
            notifier.NotifyReadReceipt(participants, readReceipt);
        }

        private void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            notifier.NotifyNewConversation(conversation);
        }

        public async Task<List<ConversationDTO>> FetchConversations()
        {
            List<ConversationDTO> conversationList = new List<ConversationDTO>();
            var systemLookupCache = new Dictionary<int, bool>();

            var fetchedConversations = await ConversationRepository.GetConversationsForUser(UserId);
            cachedConversations = fetchedConversations;

            foreach (var conversation in fetchedConversations)
            {
                ConversationDTO conversationDto = ConversationToConversationDTO(conversation);
                bool hasRealOtherParticipant = false;

                foreach (var participant in conversationDto.Participants)
                {
                    if (participant.UserId == UserId)
                    {
                        continue;
                    }

                    if (!systemLookupCache.TryGetValue(participant.UserId, out bool isSystemUser))
                    {
                        var user = await userRepository.GetById(participant.UserId);
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
                .Where(participantId => participantId != UserId)
                .Distinct()
                .ToList();

            if (otherParticipantIds.Count == 0)
            {
                return "Unknown User";
            }

            foreach (var otherUserId in otherParticipantIds)
            {
                var user = await userRepository.GetById(otherUserId);
                if (user is not null &&
                    !string.Equals(user.Username, "System", StringComparison.OrdinalIgnoreCase))
                {
                    return FormatUserDisplayName(user);
                }
            }

            var fallbackUser = await userRepository.GetById(otherParticipantIds.First());
            if (fallbackUser is null ||
                string.Equals(fallbackUser.Username, "System", StringComparison.OrdinalIgnoreCase))
            {
                return "Unknown User";
            }

            return FormatUserDisplayName(fallbackUser);
        }

        public string GetOtherUserNameByMessageDTO(MessageDataTransferObject message)
        {
            int otherUserId = message.SenderId == UserId ? message.ReceiverId : message.SenderId;
            if (otherUserId <= 0)
            {
                return "Unknown User";
            }

            return $"User {otherUserId}";
        }

        public async Task SendMessage(MessageDataTransferObject message)
        {
            Message persisted = await ConversationRepository.HandleNewMessage(MessageDTOToMessage(message));

            // Track the sent ID so the poller won't fire a duplicate notification for it.
            recentlySentMessageIds.Add(persisted.MessageId);

            // Immediately update the local cache so the poller sees the message as known.
            var cachedConv = cachedConversations.FirstOrDefault(c => c.ConversationId == persisted.ConversationId);
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

            await NotifySubscribersAboutMessage(persisted);
        }

        public async Task<int> CreateConversation(int senderId, int receiverId)
        {
            int conversationId = await ConversationRepository.CreateConversation(senderId, receiverId);
            Conversation createdConversation = await ConversationRepository.GetConversationById(conversationId);
            NotifySubscribersAboutNewConversation(createdConversation);
            return conversationId;
        }

        public async Task UpdateMessage(MessageDataTransferObject message)
        {
            Message? persisted = await ConversationRepository.HandleMessageUpdate(MessageDTOToMessage(message));
            if (persisted != null)
            {
                await NotifySubscribersAboutMessageUpdate(persisted);
            }
        }

        public async Task SendReadReceipt(ConversationDTO conversation)
        {
            var readReceipt = new ReadReceiptDTO(
                conversation.Id,
                UserId,
                conversation.Participants.First(participantItem => participantItem.UserId != UserId).UserId,
                DateTime.Now);
            await ConversationRepository.HandleReadReceipt(readReceipt);
            await NotifySubscribersAboutReadReceipt(readReceipt);
        }

        public async Task OnCardPaymentSelected(int messageId)
        {
            await FinalizeRentalRequest(messageId);
        }

        public async Task OnCashPaymentSelected(int messageId, int paymentId)
        {
            await FinalizeRentalRequest(messageId);
            await SendCashAgreementMessage(messageId, paymentId);
        }

        private async Task FinalizeRentalRequest(int messageId)
        {
            Message? updated = await ConversationRepository.HandleRentalRequestFinalization(messageId);
            if (updated != null)
            {
                await NotifySubscribersAboutMessageUpdate(updated);
            }
        }

        private async Task SendCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            Message? created = await ConversationRepository.CreateCashAgreementMessage(messageIdOfParentRentalRequestMessage, paymentId);
            if (created != null)
            {
                await NotifySubscribersAboutMessage(created);
            }
        }

        public void StartPolling()
        {
            if (pollingCancellationTokenSource != null)
            {
                return;
            }

            pollingCancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(() => PollConversationsLoop(pollingCancellationTokenSource.Token));
        }

        public void StopPolling()
        {
            pollingCancellationTokenSource?.Cancel();
            pollingCancellationTokenSource?.Dispose();
            pollingCancellationTokenSource = null;
        }

        private async Task PollConversationsLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(0.1));
                    var fetchedConversations = await ConversationRepository.GetConversationsForUser(UserId);

                    foreach (var fetchedConv in fetchedConversations)
                    {
                        var cachedConv = cachedConversations.FirstOrDefault(c => c.ConversationId == fetchedConv.ConversationId);

                        if (cachedConv == null)
                        {
                            NotifySubscribersAboutNewConversation(fetchedConv);
                        }
                        else
                        {
                            foreach (var fetchedMsg in fetchedConv.Messages)
                            {
                                var cachedMsg = cachedConv.Messages.FirstOrDefault(message => message.MessageId == fetchedMsg.MessageId);
                                if (cachedMsg == null)
                                {
                                    // Only notify if we didn't just send this message ourselves.
                                    if (!recentlySentMessageIds.Remove(fetchedMsg.MessageId))
                                    {
                                        await NotifySubscribersAboutMessage(fetchedMsg);
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
                                        await NotifySubscribersAboutMessageUpdate(fetchedMsg);
                                    }
                                }
                            }
                        }
                    }

                    cachedConversations = fetchedConversations;
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
            MessageDataTransferObject messageDTO = MessageToMessageDTO(message);
            string userName = GetOtherUserNameByMessageDTO(messageDTO);
            ActionMessageProcessed?.Invoke(messageDTO, userName);
        }

        public async Task OnConversationReceived(Conversation conversation)
        {
            ConversationDTO conversationDTO = ConversationToConversationDTO(conversation);
            string userName = await GetOtherUserNameByConversationDTO(conversationDTO);
            ActionConversationProcessed?.Invoke(conversationDTO, userName);
        }

        public void OnReadReceiptReceived(ReadReceiptDTO readReceipt)
        {
            ActionReadReceiptProcessed?.Invoke(readReceipt);
        }

        public void OnMessageUpdateReceived(Message message)
        {
            MessageDataTransferObject messageDTO = MessageToMessageDTO(message);
            string userName = GetOtherUserNameByMessageDTO(messageDTO);
            ActionMessageUpdateProcessed?.Invoke(messageDTO, userName);
        }

        public Message MessageDTOToMessage(MessageDataTransferObject messageDto)
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

        public MessageDataTransferObject MessageToMessageDTO(Message message)
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

            return new MessageDataTransferObject(
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
                .Select(messageItem => MessageToMessageDTO(messageItem))
                .ToList();

            var participantsOrdered = conversation.Participants
                .OrderBy(participantItem => participantItem.UserId)
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
