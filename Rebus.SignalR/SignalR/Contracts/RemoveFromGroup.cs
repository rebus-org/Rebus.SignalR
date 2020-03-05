using Microsoft.AspNetCore.SignalR;

namespace Rebus.SignalR.Contracts
{
	/// <summary>
	/// Removes a connection from a specific group
	/// </summary>
	/// <typeparam name="THub"></typeparam>
	public class RemoveFromGroup<THub> : GroupManagementBase<THub>
		where THub : Hub
	{
		/// <summary>
		/// <inheritdoc/>
		/// </summary>
		public RemoveFromGroup(string serverName, string groupName, string connectionId) : base(serverName, groupName, connectionId)
		{
		}
	}
}