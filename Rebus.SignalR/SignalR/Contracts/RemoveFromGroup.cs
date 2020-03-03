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
		/// Constructor
		/// </summary>
		/// <param name="serverName"></param>
		/// <param name="groupName"></param>
		/// <param name="connectionId"></param>
		public RemoveFromGroup(string serverName, string groupName, string connectionId) : base(serverName, groupName, connectionId)
		{
		}
	}
}