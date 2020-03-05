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
        /// Constructor
        /// </summary>
        /// <param name="serverName">The name of the server, that replies with an Ack</param>
        public Ack(string serverName)
        {
            ServerName = serverName;
        }

        /// <summary>
        /// The name of the server, that replies with an Ack
        /// </summary>
        public string ServerName { get; }
    }
}
