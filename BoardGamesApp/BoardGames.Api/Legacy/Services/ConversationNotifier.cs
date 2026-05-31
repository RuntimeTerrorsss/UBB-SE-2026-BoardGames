using BoardGames.Data.Models;

namespace BoardGames.Api.Legacy.Services
{
    public class ConversationNotifier : IConversationNotifier
    {
        private readonly object subscribersLock = new object();
        private readonly Dictionary<int, IConversationService> subscribers = new Dictionary<int, IConversationService>();

        public void Register(int userId, IConversationService observer)
        {
            lock (this.subscribersLock)
            {
                this.subscribers[userId] = observer;
            }
        }

        public void Unregister(int userId)
        {
            lock (this.subscribersLock)
            {
                this.subscribers.Remove(userId);
            }
        }

        public void NotifyMessage(IEnumerable<int> participantUserIds, Message message)
        {
            foreach (IConversationService observer in this.SnapshotSubscribers(participantUserIds))
            {
                observer.OnMessageReceived(message);
            }
        }

        public void NotifyMessageUpdate(IEnumerable<int> participantUserIds, Message message)
        {
            foreach (IConversationService observer in this.SnapshotSubscribers(participantUserIds))
            {
                observer.OnMessageUpdateReceived(message);
            }
        }

        public void NotifyReadReceipt(IEnumerable<int> participantUserIds, ReadReceiptDTO readReceipt)
        {
            foreach (IConversationService observer in this.SnapshotSubscribers(participantUserIds))
            {
                observer.OnReadReceiptReceived(readReceipt);
            }
        }

        public void NotifyNewConversation(Conversation conversation)
        {
            IEnumerable<int> participantUserIds = conversation.Participants.Select(participant => participant.UserId);
            foreach (IConversationService observer in this.SnapshotSubscribers(participantUserIds))
            {
                observer.OnConversationReceived(conversation);
            }
        }

        private List<IConversationService> SnapshotSubscribers(IEnumerable<int> userIds)
        {
            var observers = new List<IConversationService>();
            var distinctIds = userIds.Distinct().ToList();

            lock (this.subscribersLock)
            {
                foreach (int userId in distinctIds)
                {
                    if (this.subscribers.TryGetValue(userId, out IConversationService? observer))
                    {
                        observers.Add(observer);
                    }
                }
            }

            return observers;
        }
    }
}
