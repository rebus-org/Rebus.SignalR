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
        /// <param name="connectionId"></param>
        /// <param name="messages"></param>
        public Connection(string connectionId, IDictionary<string, byte[]> messages)
        {
            ConnectionId = connectionId;
            Messages = messages;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionId { get; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, byte[]> Messages { get; }
    }
}
