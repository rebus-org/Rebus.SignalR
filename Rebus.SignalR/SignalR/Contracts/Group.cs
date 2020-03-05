using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Rebus.SignalR.Contracts
{
    /// <summary>
    /// Sends a message to a specific group except excluded connections
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    public class Group<THub> 
        where THub : Hub
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupName">Group name</param>
        /// <param name="excludedConnectionIds">Excluded connections</param>
        /// <param name="messages">Messages to send</param>
        public Group(string groupName, string[] excludedConnectionIds, IDictionary<string, byte[]> messages)
        {
            GroupName = groupName;
            ExcludedConnectionIds = excludedConnectionIds;
            Messages = messages;
        }

        /// <summary>
        /// Group name
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// Excluded connections
        /// </summary>
        public string[] ExcludedConnectionIds { get; }

        /// <summary>
        /// Messages to send
        /// </summary>
        public IDictionary<string, byte[]> Messages { get; }
    }
}
