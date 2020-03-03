using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Internals;
using Rebus.SignalR.Contracts;
using System;
using System.Threading.Tasks;

namespace Rebus.SignalR.Handlers
{
	/// <summary>
	/// Handles Connection message
	/// </summary>
	/// <typeparam name="THub"></typeparam>
	public class ConnectionHandler<THub> : IHandleMessages<Connection<THub>>
		where THub : Hub
	{
		private readonly IRebusHubLifetimeManager _hubLifetimeManager;
		private readonly ILogger _logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="hubLifetimeManager"></param>
		/// <param name="logger"></param>
		public ConnectionHandler(HubLifetimeManager<THub> hubLifetimeManager, ILogger<ConnectionHandler<THub>> logger)
		{
			_hubLifetimeManager = hubLifetimeManager as IRebusHubLifetimeManager ?? throw new ArgumentNullException(nameof(hubLifetimeManager), "HubLifetimeManager<> must be of type IRebusHubLifetimeManager");
			_logger = logger;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public async Task Handle(Connection<THub> message)
		{
			var connection = _hubLifetimeManager.Connections[message.ConnectionId];
			// Connection doesn't exist on this server
			if (connection == null) 
				return; 

			try
			{
				var serializedHubMessage = message.Messages.ToSerializedHubMessage();
				await connection.WriteAsync(serializedHubMessage).AsTask();
			}
			catch (Exception e)
			{
				_logger.LogWarning(e, "Failed to write message");
			}
		}
	}
}
