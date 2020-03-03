using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Rebus.SignalR.Contracts
{
    /// <summary>
    /// Sends a message to a specific group
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    public class Group<THub> 
        where THub : Hub
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="excludedConnectionIds"></param>
        /// <param name="messages"></param>
        public Group(string groupName, string[] excludedConnectionIds, IDictionary<string, byte[]> messages)
        {
            GroupName = groupName;
            ExcludedConnectionIds = excludedConnectionIds;
            Messages = messages;
        }

        /// <summary>
        /// 
        /// </summary>
        public string GroupName { get; }

        /// <summary>
        /// 
        /// </summary>
        public string[] ExcludedConnectionIds { get; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, byte[]> Messages { get; }
    }
}
