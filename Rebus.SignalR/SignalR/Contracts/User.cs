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
        /// <param name="userId"></param>
        /// <param name="messages"></param>
        public User(string userId, IDictionary<string, byte[]> messages)
        {
            UserId = userId;
            Messages = messages;
        }

        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, byte[]> Messages { get; }
    }
}
