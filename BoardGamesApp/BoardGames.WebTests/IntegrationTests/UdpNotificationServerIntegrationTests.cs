#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1600 // Elements should be documented
#pragma warning disable SA1633 // The file header is missing
#pragma warning disable SA1518 // File is required to end with a single newline character
#pragma warning disable SA1028 // Code should not contain trailing whitespace
#pragma warning disable SA1513 // Closing brace should be followed by blank line
#pragma warning disable SA1402 // File may only contain a single type

using System;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NotificationServer;
using ServerCommunication;
using Xunit;

namespace BoardGames.WebTests.IntegrationTests
{
    public class UdpNotificationServerIntegrationTests : IAsyncLifetime
    {
        private CancellationTokenSource _cts;
        private Task _serverTask;
        private const int TestPort = 4555; // Using a dedicated port for testing
        private IPEndPoint _serverEndpoint;

        public Task InitializeAsync()
        {
            _cts = new CancellationTokenSource();
            _serverEndpoint = new IPEndPoint(IPAddress.Loopback, TestPort);
            
            // Act: Start the server in a background task
            _serverTask = Task.Run(async () => 
            {
                await UdpNotificationServer.ListenAsync(_cts.Token, TestPort);
            });

            // Give the server a moment to bind the socket
            Thread.Sleep(200);

            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _cts?.Cancel();
            UdpNotificationServer.Stop();
            
            if (_serverTask != null)
            {
                try
                {
                    await _serverTask;
                }
                catch
                {
                    // Ignore cancellation/stopping exceptions
                }
            }
            
            _cts?.Dispose();
        }

        [Fact]
        public async Task Server_SuccessfullyRoutesNotification_ToSubscribedClient()
        {
            // Arrange
            int targetUserId = 123;
            using var clientA = new UdpClient(0); // Client A (Receiver) binds to random ephemeral port
            using var clientB = new UdpClient(0); // Client B (Sender API) binds to random ephemeral port

            // 1. Client A subscribes
            var subscribeMsg = new SubscribeToServerMessage { UserId = targetUserId };
            byte[] subscribeBytes = CommunicationHelper.SerializeMessage(subscribeMsg);
            await clientA.SendAsync(subscribeBytes, subscribeBytes.Length, _serverEndpoint);

            // Wait a moment for server to process the subscription
            await Task.Delay(100);

            var notificationPayload = new SendNotificationMessage 
            { 
                UserId = targetUserId, 
                Title = "Test Notification", 
                Body = "This is a test notification body" 
            };
            byte[] sendBytes = CommunicationHelper.SerializeMessage(notificationPayload);

            // Act
            // 2. Client B sends a notification targeted to Client A via the Server
            await clientB.SendAsync(sendBytes, sendBytes.Length, _serverEndpoint);

            // Assert
            // 3. Client A should receive the forwarded notification!
            clientA.Client.ReceiveTimeout = 2000; // 2 seconds timeout
            
            IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes;
            try
            {
                var result = await clientA.ReceiveAsync();
                receivedBytes = result.Buffer;
            }
            catch (SocketException)
            {
                Assert.Fail("Did not receive forwarded notification packet within the timeout period.");
                return;
            }

            var receivedWrapper = CommunicationHelper.GetMessageWrapper(receivedBytes);
            Assert.NotNull(receivedWrapper);
            Assert.Equal(nameof(SendNotificationMessage), receivedWrapper.Type);

            var receivedNotification = receivedWrapper.Deserialize<SendNotificationMessage>();
            Assert.NotNull(receivedNotification);
            Assert.Equal(targetUserId, receivedNotification.UserId);
            Assert.Equal("Test Notification", receivedNotification.Title);
            Assert.Equal("This is a test notification body", receivedNotification.Body);
        }

        [Fact]
        public async Task Server_DoesNotCrash_WhenRoutingToUnsubscribedClient()
        {
            // Arrange
            int offlineUserId = 999;
            using var clientB = new UdpClient(0);

            var notificationPayload = new SendNotificationMessage 
            { 
                UserId = offlineUserId, 
                Title = "Lost Notification", 
                Body = "This user does not exist in the dictionary" 
            };
            byte[] sendBytes = CommunicationHelper.SerializeMessage(notificationPayload);

            // Act
            // Send the notification to an unsubscribed user
            await clientB.SendAsync(sendBytes, sendBytes.Length, _serverEndpoint);

            // Give the server time to process and potentially crash
            await Task.Delay(200);

            // Assert
            // The server task should still be running and not faulted!
            Assert.False(_serverTask.IsFaulted, "Server crashed when processing offline client notification.");
            Assert.False(_serverTask.IsCompleted, "Server unexpectedly stopped running.");

            // Verification: Server is still alive and can process valid requests
            int activeUserId = 555;
            using var clientA = new UdpClient(0);
            
            // Subscribe active user
            var subMsg = new SubscribeToServerMessage { UserId = activeUserId };
            var subBytes = CommunicationHelper.SerializeMessage(subMsg);
            await clientA.SendAsync(subBytes, subBytes.Length, _serverEndpoint);
            await Task.Delay(100);

            // Send notification to active user
            var validPayload = new SendNotificationMessage { UserId = activeUserId, Title = "Alive", Body = "Server works!" };
            var validBytes = CommunicationHelper.SerializeMessage(validPayload);
            await clientB.SendAsync(validBytes, validBytes.Length, _serverEndpoint);

            // Active user should receive it, confirming server is completely healthy
            clientA.Client.ReceiveTimeout = 2000;
            var result = await clientA.ReceiveAsync();
            var receivedWrapper = CommunicationHelper.GetMessageWrapper(result.Buffer);
            Assert.NotNull(receivedWrapper);
            Assert.Equal(nameof(SendNotificationMessage), receivedWrapper.Type);
        }
    }
}
