using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Internals;
using Rebus.SignalR.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rebus.SignalR.Handlers
{
	/// <summary>
	/// Handles Group message
	/// </summary>
	/// <typeparam name="THub"></typeparam>
	public class GroupHandler<THub> : IHandleMessages<Group<THub>>
		where THub : Hub
	{
		private readonly IRebusHubLifetimeManager _hubLifetimeManager;
		private readonly ILogger _logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="hubLifetimeManager"></param>
		/// <param name="logger"></param>
		public GroupHandler(HubLifetimeManager<THub> hubLifetimeManager, ILogger<GroupHandler<THub>> logger)
		{
			_hubLifetimeManager = hubLifetimeManager as IRebusHubLifetimeManager ?? throw new ArgumentNullException(nameof(hubLifetimeManager), "HubLifetimeManager<> must be of type IRebusHubLifetimeManager");
			_logger = logger;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public async Task Handle(Group<THub> message)
		{
			var serializedHubMessage = new Lazy<SerializedHubMessage>(message.Messages.ToSerializedHubMessage);

			if (!_hubLifetimeManager.GroupConnections.TryGetValue(message.GroupName, out var groupStore) || groupStore.Count == 0)
				return;

			var tasks = new List<Task>();
			foreach (var connection in groupStore)
			{
				if (message.ExcludedConnectionIds == null || !message.ExcludedConnectionIds.Contains(connection.ConnectionId, StringComparer.OrdinalIgnoreCase))
				{
					tasks.Add(connection.WriteAsync(serializedHubMessage.Value).AsTask());
				}
			}

			try
			{
				await Task.WhenAll(tasks);
			}
			catch (Exception e)
			{
				_logger.LogWarning(e, "Failed to write message");
			}
		}
	}
}
