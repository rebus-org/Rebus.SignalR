using Microsoft.AspNetCore.SignalR;

namespace Rebus.SignalR.Contracts
{
    /// <summary>
    /// Acknowledgement reply
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    public class Ack<THub> 
        where THub : Hub
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverName"></param>
        public Ack(string serverName)
        {
            ServerName = serverName;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ServerName { get; }
    }
}
