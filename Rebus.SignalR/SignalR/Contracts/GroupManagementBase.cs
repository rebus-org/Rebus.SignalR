using Microsoft.AspNetCore.SignalR;

namespace Rebus.SignalR.Contracts
{
    /// <summary>
    /// Base class for group management commands
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    public abstract class GroupManagementBase<THub> 
        where THub : Hub
    {
        /// <summary>
        /// Constructor
        /// </summary>
		/// <param name="serverName">The name of the server, that sent a request</param>
		/// <param name="groupName">Group name</param>
		/// <param name="connectionId">Connection identifier</param>
        public GroupManagementBase(string serverName, string groupName, string connectionId)
        {
            ServerName = serverName;
            GroupName = groupName;
            ConnectionId = connectionId;
        }

        /// <summary>
        /// The name of the server, that sent a request
        /// </summary>
        public string ServerName { get; }

        /// <summary>
        /// Group name
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Connection identifier
        /// </summary>
        public string ConnectionId { get; }
    }
}
