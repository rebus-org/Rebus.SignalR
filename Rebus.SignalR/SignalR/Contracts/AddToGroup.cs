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
		/// Constructor
		/// </summary>
		/// <param name="serverName"></param>
		/// <param name="groupName"></param>
		/// <param name="connectionId"></param>
		public AddToGroup(string serverName, string groupName, string connectionId) : base(serverName, groupName, connectionId)
		{
		}
	}
}