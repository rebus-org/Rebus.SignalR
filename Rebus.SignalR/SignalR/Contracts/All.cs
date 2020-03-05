using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Rebus.SignalR.Contracts
{
    /// <summary>
    /// Sends a broadcast message to all connections except excluded ones
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    public class All<THub> 
        where THub : Hub
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="excludedConnectionIds">Excluded connections</param>
        /// <param name="messages">Messages to send</param>
        public All(string[] excludedConnectionIds, IDictionary<string, byte[]> messages)
        {
            ExcludedConnectionIds = excludedConnectionIds;
            Messages = messages;
        }

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
