using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Rebus.SignalR.Contracts
{
    /// <summary>
    /// Sends a message to a specific user
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    public class User<THub> 
        where THub : Hub
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="messages">Messages to send</param>
        public User(string userId, IDictionary<string, byte[]> messages)
        {
            UserId = userId;
            Messages = messages;
        }

        /// <summary>
        /// User identifier
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Messages to send
        /// </summary>
        public IDictionary<string, byte[]> Messages { get; }
    }
}
