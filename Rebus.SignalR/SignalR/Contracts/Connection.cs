using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Rebus.SignalR.Contracts
{
    /// <summary>
    /// Sends a message to a specific connection
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    public class Connection<THub> 
        where THub : Hub
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionId">Connection identifier</param>
        /// <param name="messages">Messages to send</param>
        public Connection(string connectionId, IDictionary<string, byte[]> messages)
        {
            ConnectionId = connectionId;
            Messages = messages;
        }

        /// <summary>
        /// Connection identifier
        /// </summary>
        public string ConnectionId { get; }

        /// <summary>
        /// Messages to send
        /// </summary>
        public IDictionary<string, byte[]> Messages { get; }
    }
}
