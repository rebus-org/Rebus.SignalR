using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Internals;
using Rebus.SignalR.Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rebus.SignalR
{
	/// <summary>
	/// Rebus Hub Lifetime Manager
	/// </summary>
	/// <typeparam name="THub"></typeparam>
	public class RebusHubLifetimeManager<THub> : HubLifetimeManager<THub>, IRebusHubLifetimeManager
		where THub : Hub
	{
		private readonly IBus _bus;
		private readonly ILogger _logger;

		private readonly IReadOnlyList<IHubProtocol> _protocols;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="bus"></param>
		/// <param name="hubProtocolResolver"></param>
		/// <param name="logger"></param>
		public RebusHubLifetimeManager(IBus bus, IHubProtocolResolver hubProtocolResolver, ILogger<RebusHubLifetimeManager<THub>> logger)
		{
			_bus = bus;
			_logger = logger;
			_protocols = hubProtocolResolver.AllProtocols;

			Connections = new HubConnectionStore();
			GroupConnections = new ConcurrentDictionary<string, HubConnectionStore>();
			UserConnections = new ConcurrentDictionary<string, HubConnectionStore>();
			ServerName = $"{Environment.MachineName}-{Guid.NewGuid():N}";
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public string ServerName { get; }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public HubConnectionStore Connections { get; }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public ConcurrentDictionary<string, HubConnectionStore> GroupConnections { get; }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public ConcurrentDictionary<string, HubConnectionStore> UserConnections { get; }

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override async Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
		{
			if (connectionId == null)
				throw new ArgumentNullException(nameof(connectionId));

			if (groupName == null)
				throw new ArgumentNullException(nameof(groupName));

			try
			{
				_logger.LogInformation("Publishing AddToGroup<THub> {GroupName} message to Rebus for {ConnectionId}.", groupName, connectionId);

				var command = new AddToGroup<THub>(serverName: ServerName, groupName: groupName, connectionId: connectionId);
				var ack = await _bus.PublishRequest<Ack<THub>>(command, externalCancellationToken: cancellationToken).ConfigureAwait(false);

				_logger.LogInformation("Receved response to AddToGroup<THub> {GroupName} message for {ConnectionId} from {ServerName}.", groupName, connectionId, ack.ServerName);
			}
			catch (TaskCanceledException e)
			{
				_logger.LogWarning(e, "AddToGroup<THub> {GroupName} for {ConnectionId} ack timed out.", groupName, connectionId);
			}
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task OnConnectedAsync(HubConnectionContext connection)
		{
			var feature = new RebusFeature();
			connection.Features.Set(feature);

			Connections.Add(connection);
			if (!string.IsNullOrEmpty(connection.UserIdentifier))
			{
				var userStore = UserConnections.GetOrAdd(connection.UserIdentifier, _ => new HubConnectionStore());
				userStore.Add(connection);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task OnDisconnectedAsync(HubConnectionContext connection)
		{
			Connections.Remove(connection);
			if (!string.IsNullOrEmpty(connection.UserIdentifier))
			{
				HubConnectionStore userStore; 
				if (UserConnections.TryGetValue(connection.UserIdentifier, out userStore))
				{
					userStore.Remove(connection);
				}
			}

			var groups = connection.Features.Get<RebusFeature>().Groups;

			foreach (var group in groups.ToArray())
			{
				RemoveFromGroupLocal(connection, group.Key);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public void AddToGroupLocal(HubConnectionContext connection, string groupName)
		{
			var feature = connection.Features.Get<RebusFeature>();
			feature.Groups.TryAdd(groupName, true);

			var groupStore = GroupConnections.GetOrAdd(groupName, _ => new HubConnectionStore());
			groupStore.Add(connection);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public void RemoveFromGroupLocal(HubConnectionContext connection, string groupName)
		{
			var feature = connection.Features.Get<RebusFeature>();
			feature.Groups.TryRemove(groupName, out _);

			HubConnectionStore groupStore; 
			if (GroupConnections.TryGetValue(groupName, out groupStore))
			{
				groupStore.Remove(connection);
			}
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override async Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
		{
			if (connectionId == null)
				throw new ArgumentNullException(nameof(connectionId));

			if (groupName == null)
				throw new ArgumentNullException(nameof(groupName));

			try
			{
				_logger.LogInformation("Publishing RemoveGroup<THub> {GroupName} message to Rebus for {ConnectionId}.", groupName, connectionId);

				var command = new RemoveFromGroup<THub>(serverName: ServerName, groupName: groupName, connectionId: connectionId);
				var ack = await _bus.PublishRequest<Ack<THub>>(command, externalCancellationToken: cancellationToken).ConfigureAwait(false);

				_logger.LogInformation("Receved response to RemoveFromToGroup<THub> {GroupName} message for {ConnectionId} from {ServerName}.", groupName, connectionId, ack.ServerName);
			}
			catch (TaskCanceledException e)
			{
				_logger.LogWarning(e, "RemoveFromGroup<THub> {GroupName} for {ConnectionId} ack timed out.", groupName, connectionId);
			}
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
		{
			_logger.LogInformation("Publishing All<THub> message {MethodName} to Rebus.", methodName);
			var command = new All<THub>(excludedConnectionIds: null, messages: _protocols.ToProtocolDictionary(methodName, args));
			return _bus.Publish(command);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
		{
			if (excludedConnectionIds == null)
				throw new ArgumentNullException(nameof(excludedConnectionIds));

			_logger.LogInformation("Publishing All<THub> message {MethodName} to Rebus, with exceptions.", methodName);
			var command = new All<THub>(excludedConnectionIds: excludedConnectionIds.ToArray(), messages: _protocols.ToProtocolDictionary(methodName, args));
			return _bus.Publish(command);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default)
		{
			if (connectionId == null)
				throw new ArgumentNullException(nameof(connectionId));

			_logger.LogInformation("Publishing Connection<THub> message {MethodName} to Rebus for {ConnectionId}.", methodName, connectionId);
			var command = new Connection<THub>(connectionId: connectionId, messages: _protocols.ToProtocolDictionary(methodName, args));
			return _bus.Publish(command);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args, CancellationToken cancellationToken = default)
		{
			if (connectionIds == null)
				throw new ArgumentNullException(nameof(connectionIds));

			if (connectionIds.Count > 0)
			{
				var messages = _protocols.ToProtocolDictionary(methodName, args);
				var publishTasks = new List<Task>(connectionIds.Count);

				_logger.LogInformation("Publishing multiple Connection<THub> messages {MethodName} to Rebus.", methodName);
				foreach (var connectionId in connectionIds)
				{
					var command = new Connection<THub>(connectionId: connectionId, messages: messages);
					publishTasks.Add(_bus.Publish(command));
				}

				return Task.WhenAll(publishTasks);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default)
		{
			if (groupName == null)
				throw new ArgumentNullException(nameof(groupName));

			_logger.LogInformation("Publishing Group<THub> message {MethodName} to Rebus for {GroupName}.", methodName, groupName);
			var command = new Group<THub>(groupName: groupName, excludedConnectionIds: null, messages: _protocols.ToProtocolDictionary(methodName, args));
			return _bus.Publish(command);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
		{
			if (groupName == null)
				throw new ArgumentNullException(nameof(groupName));
			if (excludedConnectionIds == null)
				throw new ArgumentNullException(nameof(excludedConnectionIds));

			_logger.LogInformation("Publishing Group<THub> message {MethodName} to Rebus for {GroupName} with exceptions.", methodName, groupName);
			var command = new Group<THub>(groupName: groupName, excludedConnectionIds: excludedConnectionIds.ToArray(), messages: _protocols.ToProtocolDictionary(methodName, args));
			return _bus.Publish(command);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args, CancellationToken cancellationToken = default)
		{
			if (groupNames == null)
				throw new ArgumentNullException(nameof(groupNames));

			if (groupNames.Count > 0)
			{
				var messages = _protocols.ToProtocolDictionary(methodName, args);
				var publishTasks = new List<Task>(groupNames.Count);

				_logger.LogInformation("Publishing multiple Group<THub> messages {MethodName} to Rebus.", methodName);
				foreach (var groupName in groupNames)
				{
					if (groupName != null)
					{
						var command = new Group<THub>(groupName: groupName, excludedConnectionIds: null, messages: messages);
						publishTasks.Add(_bus.Publish(command));
					}
				}

				return Task.WhenAll(publishTasks);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default)
		{
			if (userId == null)
				throw new ArgumentNullException(nameof(userId));

			_logger.LogInformation("Publishing User<THub> message {MethodName} to Rebus for {UserId}.", methodName, userId);
			var command = new User<THub>(userId: userId, messages: _protocols.ToProtocolDictionary(methodName, args));
			return _bus.Publish(command);
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public override Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args, CancellationToken cancellationToken = default)
		{
			if (userIds == null)
				throw new ArgumentNullException(nameof(userIds));

			if (userIds.Count > 0)
			{
				var messages = _protocols.ToProtocolDictionary(methodName, args);
				var publishTasks = new List<Task>(userIds.Count);

				_logger.LogInformation("Publishing multiple User<THub> messages {MethodName} to Rebus.", methodName);
				foreach (var userId in userIds)
				{
					if (userId != null)
					{
						var command = new User<THub>(userId: userId, messages: messages);
						publishTasks.Add(_bus.Publish(command));
					}
				}

				return Task.WhenAll(publishTasks);
			}

			return Task.CompletedTask;
		}
	}
}