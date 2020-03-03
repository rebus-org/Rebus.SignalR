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
        /// <param name="serverName"></param>
        /// <param name="groupName"></param>
        /// <param name="connectionId"></param>
        public GroupManagementBase(string serverName, string groupName, string connectionId)
        {
            ServerName = serverName;
            GroupName = groupName;
            ConnectionId = connectionId;
        }

        /// <summary>
        /// Gets the ServerName of the group command.
        /// </summary>
        public string ServerName { get; }

        /// <summary>
        /// Gets the group on which the action is performed.
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Gets the ID of the connection to be added or removed from the group.
        /// </summary>
        public string ConnectionId { get; }
    }
}
