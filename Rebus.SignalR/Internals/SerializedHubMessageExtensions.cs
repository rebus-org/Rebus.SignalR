using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Collections.Generic;

namespace Rebus.Internals
{
    /// <summary>
    /// 
    /// </summary>
    public static class SerializedHubMessageExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocolMessages"></param>
        /// <returns></returns>
        public static SerializedHubMessage ToSerializedHubMessage(this IDictionary<string, byte[]> protocolMessages)
        {
            var count = protocolMessages.Count;
            var serializations = new List<SerializedMessage>(count);

            foreach (var message in protocolMessages)
            {
                var protocol = message.Key;
                var serialized = message.Value;
                serializations.Add(new SerializedMessage(protocol, serialized));
            }

            return new SerializedHubMessage(serializations);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="protocols"></param>
        /// <param name="methodName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IDictionary<string, byte[]> ToProtocolDictionary(this IReadOnlyList<IHubProtocol> protocols, string methodName, object[] args)
        {
            var serializedMessageHub = new SerializedHubMessage(new InvocationMessage(methodName, args));

            var messages = new Dictionary<string, byte[]>();

            foreach (var protocol in protocols)
            {
                var serialized = serializedMessageHub.GetSerializedMessage(protocol);

                messages.Add(protocol.Name, serialized.ToArray());
            }

            return messages;
        }
    }
}