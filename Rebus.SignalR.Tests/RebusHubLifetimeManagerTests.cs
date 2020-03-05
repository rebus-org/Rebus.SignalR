using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Rebus.Bus;
using Rebus.SignalR.Contracts;
using Rebus.SignalR.Handlers;
using Rebus.SignalR.Tests.Internal;
using Rebus.SignalR.Tests.OfficialFramework;
using Rebus.TestHelpers;
using Rebus.TestHelpers.Events;
using Rebus.Tests.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rebus.SignalR.Tests
{
	[TestFixture]
	public class RebusHubLifetimeManagerTests : FixtureBase
	{
		private (RebusHubLifetimeManager<TestHub> HubLifetimeManager, IBus Bus) ArrangeRebusLifetimeManager(IBus bus = null)
		{
			var fakeBus = bus ?? new FakeBus();
			var hubProtocolResolver = new DefaultHubProtocolResolver(new IHubProtocol[] {
				new JsonHubProtocol(new OptionsWrapper<JsonHubProtocolOptions>(new JsonHubProtocolOptions()))
		   }, null);

			var hubLifetimeManager = new RebusHubLifetimeManager<TestHub>(fakeBus, hubProtocolResolver, NullLogger<RebusHubLifetimeManager<TestHub>>.Instance);

			return (hubLifetimeManager, fakeBus);
		}

		[Test]
		public async Task OnConnectedAsync_WhenUserIdentifierIsNotNull_AddsConnectionToUserStore()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: "1");

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager();

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				var userStore = hubLifetimeManager.UserConnections[connection.UserIdentifier];
				Assert.NotNull(userStore);
				Assert.True(userStore.GetEnumerator().ToEnumerable().Contains(connection));
			}
		}

		[Test]
		public async Task OnConnectedAsync_WhenUserIdentifierIsNull_LeavesUserStoreUntouched()
		{
			using (var client = new TestClient())
			{
				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager();

				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				Assert.Zero(hubLifetimeManager.UserConnections.Count);
			}
		}

		[Test]
		public async Task OnDisconnectedAsync_WhenUserIdentifierIsNotNull_RemovesConnectionFromUserStore()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: "1");

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager();

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				var userStore = hubLifetimeManager.UserConnections[connection.UserIdentifier];
				Assert.AreEqual(1, userStore.Count);

				await hubLifetimeManager.OnDisconnectedAsync(connection).OrTimeout();
				Assert.AreEqual(0, userStore.Count);
			}
		}

		[Test]
		public async Task OnDisconnectedAsync_WhenUserIdentifierIsNull_LeavesUserStoreUntouched()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager();

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();
				await hubLifetimeManager.OnDisconnectedAsync(connection).OrTimeout();

				Assert.Zero(hubLifetimeManager.UserConnections.Count);
			}
		}

		[Test]
		public async Task AddToGroupAsync_WhenConnectionDoesntExist_PublishesAddToGroupCommandToBus()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				var groupName = "group";

				using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(0.5)))
				{
					await hubLifetimeManager.AddToGroupAsync(connection.ConnectionId, groupName, cancellationTokenSource.Token);
				}

				var publishedEvent = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<AddToGroup<TestHub>>().FirstOrDefault();

				Assert.NotNull(publishedEvent);
				Assert.True(publishedEvent.ConnectionId == connection.ConnectionId && publishedEvent.GroupName == groupName);
			}
		}

		[Test]
		public async Task AddToGroupAsync_WhenConnectionExists_AddsConnectionToGroupStore()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				var groupName = "group";

				using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(0.5)))
				{
					await hubLifetimeManager.AddToGroupAsync(connection.ConnectionId, groupName, cancellationTokenSource.Token);
				}

				HubConnectionStore groupStore;
				Assert.True(hubLifetimeManager.GroupConnections.TryGetValue(groupName, out groupStore));
				Assert.True(groupStore.GetEnumerator().ToEnumerable().Contains(connection));
			}
		}

		[Test]
		public async Task RemoveFromGroupAsync_WhenConnectionDoesntExist_PublishesRemoveFromGroupCommandToBus()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				var groupName = "group";

				using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(0.5)))
				{
					await hubLifetimeManager.RemoveFromGroupAsync(connection.ConnectionId, groupName, cancellationTokenSource.Token);
				}

				var publishedEvent = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<RemoveFromGroup<TestHub>>().FirstOrDefault();

				Assert.NotNull(publishedEvent);
				Assert.True(publishedEvent.ConnectionId == connection.ConnectionId && publishedEvent.GroupName == groupName);
			}
		}

		[Test]
		public async Task RemoveFromGroupAsync_WhenConnectionExists_RemovesConnectionFromGroupStore()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				var groupName = "group";

				hubLifetimeManager.AddToGroupLocal(connection, groupName);

				using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(0.5)))
				{
					await hubLifetimeManager.RemoveFromGroupAsync(connection.ConnectionId, groupName, cancellationTokenSource.Token);
				}

				HubConnectionStore groupStore;
				Assert.True(hubLifetimeManager.GroupConnections.TryGetValue(groupName, out groupStore));
				Assert.False(groupStore.GetEnumerator().ToEnumerable().Contains(connection));
			}
		}
		[Test]
		public async Task SendAllAsync_PublishesAllCommandToBusAndHandlerWritesInvocationToClient()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				await hubLifetimeManager.SendAllAsync("Hello", new object[] { "World" });

				var publishedEvent = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<All<TestHub>>().FirstOrDefault();
				Assert.NotNull(publishedEvent);
				Assert.Null(publishedEvent.ExcludedConnectionIds);

				var allHandler = new AllHandler<TestHub>(hubLifetimeManager, NullLogger<AllHandler<TestHub>>.Instance);

				await allHandler.Handle(publishedEvent);

				var message = client.TryRead() as InvocationMessage;
				Assert.NotNull(message);
				Assert.AreEqual("Hello", message.Target);
				Assert.AreEqual(1, message.Arguments.Length);
				Assert.AreEqual("World", (string)message.Arguments[0]);
			}
		}

		[Test]
		public async Task SendAllExceptAsync_PublishesAllCommandToBusAndHandlerSkipsExcludedConnection()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				await hubLifetimeManager.SendAllExceptAsync("Hello", new object[] { "World" }, new List<string>(new string[] { connection.ConnectionId }).AsReadOnly());

				var publishedEvent = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<All<TestHub>>().FirstOrDefault();
				Assert.NotNull(publishedEvent);
				Assert.True(publishedEvent.ExcludedConnectionIds?.Length == 1 && publishedEvent.ExcludedConnectionIds[0] == connection.ConnectionId);

				var allHandler = new AllHandler<TestHub>(hubLifetimeManager, NullLogger<AllHandler<TestHub>>.Instance);

				await allHandler.Handle(publishedEvent);

				var message = client.TryRead() as InvocationMessage;
				Assert.Null(message);
			}
		}

		[Test]
		public async Task SendConnectionAsync_PublishesConnectionCommandToBusAndHandlerWritesInvocationToClient()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				await hubLifetimeManager.SendConnectionAsync(connection.ConnectionId, "Hello", new object[] { "World" });

				var publishedEvent = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<Connection<TestHub>>().FirstOrDefault();
				Assert.NotNull(publishedEvent);
				Assert.AreEqual(connection.ConnectionId, publishedEvent.ConnectionId);

				var connectionHandler = new ConnectionHandler<TestHub>(hubLifetimeManager, NullLogger<ConnectionHandler<TestHub>>.Instance);

				await connectionHandler.Handle(publishedEvent);

				var message = client.TryRead() as InvocationMessage;
				Assert.NotNull(message);
				Assert.AreEqual("Hello", message.Target);
				Assert.AreEqual(1, message.Arguments.Length);
				Assert.AreEqual("World", (string)message.Arguments[0]);
			}
		}

		[Test]
		public async Task SendConnectionsAsync_PublishesMultipleConnectionCommandsToBus()
		{
			var fakeBus = new FakeBus();

			var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

			var connectionIds = new List<string>(new string[] { "1", "2" }).AsReadOnly();

			await hubLifetimeManager.SendConnectionsAsync(connectionIds, "Hello", new object[] { "World" });

			var publishedEvents = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<Connection<TestHub>>().ToArray();
			Assert.AreEqual(2, publishedEvents.Length);
			Assert.AreEqual("1", publishedEvents[0].ConnectionId);
			Assert.AreEqual("2", publishedEvents[1].ConnectionId);
		}

		[Test]
		public async Task SendGroupAsync_PublishesGroupCommandToBusAndHandlerWritesInvocationToClient()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				var groupName = "Group";

				await hubLifetimeManager.SendGroupAsync(groupName, "Hello", new object[] { "World" });

				var publishedEvent = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<Group<TestHub>>().FirstOrDefault();
				Assert.NotNull(publishedEvent);
				Assert.Null(publishedEvent.ExcludedConnectionIds);

				var groupHandler = new GroupHandler<TestHub>(hubLifetimeManager, NullLogger<GroupHandler<TestHub>>.Instance);
				hubLifetimeManager.AddToGroupLocal(connection, groupName);

				await groupHandler.Handle(publishedEvent);

				var message = client.TryRead() as InvocationMessage;
				Assert.NotNull(message);
				Assert.AreEqual("Hello", message.Target);
				Assert.AreEqual(1, message.Arguments.Length);
				Assert.AreEqual("World", (string)message.Arguments[0]);
			}
		}

		[Test]
		public async Task SendGroupExceptAsync_PublishesGroupCommandToBusAndHandlerSkipsExcludedConnection()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				var groupName = "Group";

				await hubLifetimeManager.SendGroupExceptAsync(groupName, "Hello", new object[] { "World" }, new List<string>(new string[] { connection.ConnectionId }).AsReadOnly());

				var publishedEvent = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<Group<TestHub>>().FirstOrDefault();
				Assert.NotNull(publishedEvent);
				Assert.True(publishedEvent.ExcludedConnectionIds?.Length == 1 && publishedEvent.ExcludedConnectionIds[0] == connection.ConnectionId);

				var groupHandler = new GroupHandler<TestHub>(hubLifetimeManager, NullLogger<GroupHandler<TestHub>>.Instance);
				hubLifetimeManager.AddToGroupLocal(connection, groupName);

				await groupHandler.Handle(publishedEvent);

				var message = client.TryRead() as InvocationMessage;
				Assert.Null(message);
			}
		}

		[Test]
		public async Task SendGroupsAsync_PublishesMultipleGroupCommandsToBus()
		{
			var fakeBus = new FakeBus();

			var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

			var groupNames = new List<string>(new string[] { "Group1", "Group2" }).AsReadOnly();

			await hubLifetimeManager.SendGroupsAsync(groupNames, "Hello", new object[] { "World" });

			var publishedEvents = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<Group<TestHub>>().ToArray();
			Assert.AreEqual(2, publishedEvents.Length);
			Assert.AreEqual("Group1", publishedEvents[0].GroupName);
			Assert.AreEqual("Group2", publishedEvents[1].GroupName);
		}

		[Test]
		public async Task SendUserAsync_PublishesConnectionCommandToBusAndHandlerWritesInvocationToClient()
		{
			using (var client = new TestClient())
			{
				var userId = "1";

				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: userId);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				await hubLifetimeManager.SendUserAsync(userId, "Hello", new object[] { "World" });

				var publishedEvent = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<User<TestHub>>().FirstOrDefault();
				Assert.NotNull(publishedEvent);
				Assert.AreEqual(userId, publishedEvent.UserId);

				var userHandler = new UserHandler<TestHub>(hubLifetimeManager, NullLogger<UserHandler<TestHub>>.Instance);

				await userHandler.Handle(publishedEvent);

				var message = client.TryRead() as InvocationMessage;
				Assert.NotNull(message);
				Assert.AreEqual("Hello", message.Target);
				Assert.AreEqual(1, message.Arguments.Length);
				Assert.AreEqual("World", (string)message.Arguments[0]);
			}
		}

		[Test]
		public async Task SendUsersAsync_PublishesMultipleGroupCommandsToBus()
		{
			var fakeBus = new FakeBus();

			var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

			var userIds = new List<string>(new string[] { "1", "2" }).AsReadOnly();

			await hubLifetimeManager.SendUsersAsync(userIds, "Hello", new object[] { "World" });

			var publishedEvents = fakeBus.Events.OfType<MessagePublished>().Select(m => m.EventMessage).OfType<User<TestHub>>().ToArray();
			Assert.AreEqual(2, publishedEvents.Length);
			Assert.AreEqual("1", publishedEvents[0].UserId);
			Assert.AreEqual("2", publishedEvents[1].UserId);
		}

		[Test]
		public async Task AddToGroupHandlerHandle_WhenConnectionExists_AddsConnectionToGroupStoreAndRepliesToBus()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				var addToGroupCommand = new AddToGroup<TestHub>("localhost", "group", connection.ConnectionId);

				var addToGroupHandler = new AddToGroupHandler<TestHub>(hubLifetimeManager, fakeBus);

				await addToGroupHandler.Handle(addToGroupCommand);

				HubConnectionStore groupStore;
				Assert.True(hubLifetimeManager.GroupConnections.TryGetValue(addToGroupCommand.GroupName, out groupStore));
				Assert.True(groupStore.GetEnumerator().ToEnumerable().Contains(connection));

				var ackReply = fakeBus.Events.OfType<ReplyMessageSent>().Select(m => m.ReplyMessage).OfType<Ack<TestHub>>().FirstOrDefault();
				Assert.NotNull(ackReply);
				Assert.AreEqual(hubLifetimeManager.ServerName, ackReply.ServerName);
			}
		}

		[Test]
		public async Task AddToGroupHandlerHandle_WhenConnectionDoesntExist_LeavesGroupStoreUntouched()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				var addToGroupCommand = new AddToGroup<TestHub>("localhost", "group", connection.ConnectionId);

				var addToGroupHandler = new AddToGroupHandler<TestHub>(hubLifetimeManager, fakeBus);

				await addToGroupHandler.Handle(addToGroupCommand);

				HubConnectionStore groupStore;
				Assert.False(hubLifetimeManager.GroupConnections.TryGetValue(addToGroupCommand.GroupName, out groupStore));
			}
		}

		[Test]
		public async Task RemoveFromGroupHandlerHandle_WhenConnectionExists_RemovesConnectionFromGroupStoreAndRepliesToBus()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				await hubLifetimeManager.OnConnectedAsync(connection).OrTimeout();

				var removeFromGroupCommand = new RemoveFromGroup<TestHub>("localhost", "group", connection.ConnectionId);

				hubLifetimeManager.AddToGroupLocal(connection, removeFromGroupCommand.GroupName);

				var removeFromGroupHandler = new RemoveFromGroupHandler<TestHub>(hubLifetimeManager, fakeBus);

				await removeFromGroupHandler.Handle(removeFromGroupCommand);

				HubConnectionStore groupStore;
				Assert.True(hubLifetimeManager.GroupConnections.TryGetValue(removeFromGroupCommand.GroupName, out groupStore));
				Assert.False(groupStore.GetEnumerator().ToEnumerable().Contains(connection));

				var ackReply = fakeBus.Events.OfType<ReplyMessageSent>().Select(m => m.ReplyMessage).OfType<Ack<TestHub>>().FirstOrDefault();
				Assert.NotNull(ackReply);
				Assert.AreEqual(hubLifetimeManager.ServerName, ackReply.ServerName);
			}
		}

		[Test]
		public async Task RemoveFromGroupHandlerHandle_WhenConnectionDoesntExist_LeavesGroupStoreUntouched()
		{
			using (var client = new TestClient())
			{
				var connection = HubConnectionContextUtils.Create(connection: client.Connection, userIdentifier: null);

				var fakeBus = new FakeBus();

				var (hubLifetimeManager, bus) = ArrangeRebusLifetimeManager(fakeBus);

				var removeFromGroupCommand = new RemoveFromGroup<TestHub>("localhost", "group", connection.ConnectionId);

				var removeFromGroupHandler = new RemoveFromGroupHandler<TestHub>(hubLifetimeManager, fakeBus);

				await removeFromGroupHandler.Handle(removeFromGroupCommand);

				HubConnectionStore groupStore;
				Assert.False(hubLifetimeManager.GroupConnections.TryGetValue(removeFromGroupCommand.GroupName, out groupStore));
			}
		}
	}
}
