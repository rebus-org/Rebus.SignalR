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
	/// Handles broadcast All message
	/// </summary>
	/// <typeparam name="THub"></typeparam>
	public class AllHandler<THub> : IHandleMessages<All<THub>>
		where THub : Hub
	{
		private readonly IRebusHubLifetimeManager _hubLifetimeManager;
		private readonly ILogger _logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="hubLifetimeManager"></param>
		/// <param name="logger"></param>
		public AllHandler(HubLifetimeManager<THub> hubLifetimeManager, ILogger<AllHandler<THub>> logger)
		{
			_hubLifetimeManager = hubLifetimeManager as IRebusHubLifetimeManager ?? throw new ArgumentNullException(nameof(hubLifetimeManager), "HubLifetimeManager<> must be of type IRebusHubLifetimeManager");
			_logger = logger;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public async Task Handle(All<THub> message)
		{
			var serializedHubMessage = new Lazy<SerializedHubMessage>(message.Messages.ToSerializedHubMessage);

			var tasks = new List<Task>(_hubLifetimeManager.Connections.Count);

			foreach (var connection in _hubLifetimeManager.Connections)
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
