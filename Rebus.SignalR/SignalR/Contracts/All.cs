using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;

namespace Rebus.SignalR.Contracts
{
    /// <summary>
    /// A broadcast message contract
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    public class All<THub> 
        where THub : Hub
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="excludedConnectionIds"></param>
        /// <param name="messages"></param>
        public All(string[] excludedConnectionIds, IDictionary<string, byte[]> messages)
        {
            ExcludedConnectionIds = excludedConnectionIds;
            Messages = messages;
        }

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
