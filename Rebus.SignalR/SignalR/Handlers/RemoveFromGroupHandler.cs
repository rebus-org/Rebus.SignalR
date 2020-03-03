using Microsoft.AspNetCore.SignalR;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.SignalR.Contracts;
using System;
using System.Threading.Tasks;

namespace Rebus.SignalR.Handlers
{
	/// <summary>
	/// Handles RemoveFromGroup message
	/// </summary>
	/// <typeparam name="THub"></typeparam>
	public class RemoveFromGroupHandler<THub> : IHandleMessages<RemoveFromGroup<THub>>
		where THub : Hub
	{
		private readonly IRebusHubLifetimeManager _hubLifetimeManager;
		private readonly IBus _bus;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="hubLifetimeManager"></param>
		/// <param name="bus"></param>
		public RemoveFromGroupHandler(HubLifetimeManager<THub> hubLifetimeManager, IBus bus)
		{
			_hubLifetimeManager = hubLifetimeManager as IRebusHubLifetimeManager ?? throw new ArgumentNullException(nameof(hubLifetimeManager), "HubLifetimeManager<> must be of type IRebusHubLifetimeManager");
			_bus = bus;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public Task Handle(RemoveFromGroup<THub> message)
		{
			var connection = _hubLifetimeManager.Connections[message.ConnectionId];

			// Connection doesn't exist on this server
			if (connection == null)
				return Task.CompletedTask;

			_hubLifetimeManager.RemoveFromGroupLocal(connection, message.GroupName);

			return _bus.Reply(new Ack<THub>(serverName: _hubLifetimeManager.ServerName));
		}
	}
}
