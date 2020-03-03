using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;
using Rebus.Internals;
using Rebus.SignalR.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rebus.SignalR.Handlers
{
	/// <summary>
	/// Handles User message
	/// </summary>
	/// <typeparam name="THub"></typeparam>
	public class UserHandler<THub> : IHandleMessages<User<THub>>
		where THub : Hub
	{
		private readonly IRebusHubLifetimeManager _hubLifetimeManager;
		private readonly ILogger _logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="hubLifetimeManager"></param>
		/// <param name="logger"></param>
		public UserHandler(HubLifetimeManager<THub> hubLifetimeManager, ILogger<UserHandler<THub>> logger)
		{
			_hubLifetimeManager = hubLifetimeManager as IRebusHubLifetimeManager ?? throw new ArgumentNullException(nameof(hubLifetimeManager), "HubLifetimeManager<> must be of type IRebusHubLifetimeManager");
			_logger = logger;
		}

		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public async Task Handle(User<THub> message)
		{
			var serializedHubMessage = new Lazy<SerializedHubMessage>(message.Messages.ToSerializedHubMessage);

			var userStore = _hubLifetimeManager.UserConnections[message.UserId];

			if (userStore == null || userStore.Count == 0)
				return;

			var tasks = new List<Task>();
			foreach (var connection in userStore)
			{
				tasks.Add(connection.WriteAsync(serializedHubMessage.Value).AsTask());
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
