using Microsoft.AspNetCore.SignalR;

namespace Rebus.SignalR.Contracts
{
	/// <summary>
	/// Adds a connection to a specific group
	/// </summary>
	/// <typeparam name="THub"></typeparam>
	public class AddToGroup<THub> : GroupManagementBase<THub>
		where THub : Hub
	{
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public AddToGroup(string serverName, string groupName, string connectionId) : base(serverName, groupName, connectionId)
		{
		}
	}
}