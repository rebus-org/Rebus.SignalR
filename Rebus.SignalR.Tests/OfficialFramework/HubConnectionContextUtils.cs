using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rebus.SignalR.Tests.OfficialFramework
{
    internal static class HubConnectionContextUtils
    {
        public static HubConnectionContext Create(ConnectionContext connection, IHubProtocol protocol = null, string userIdentifier = null)
        {
            var contextOptions = new HubConnectionContextOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(15),
            };

            return new HubConnectionContext(connection, contextOptions, NullLoggerFactory.Instance)
            {
                Protocol = protocol ?? new JsonHubProtocol(),
                UserIdentifier = userIdentifier,
            };
        }

        public static MockHubConnectionContext CreateMock(ConnectionContext connection)
        {
            var contextOptions = new HubConnectionContextOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(15),
                ClientTimeoutInterval = TimeSpan.FromSeconds(15),
                StreamBufferCapacity = 10,
            };
            return new MockHubConnectionContext(connection, contextOptions, NullLoggerFactory.Instance);
        }

        public class MockHubConnectionContext : HubConnectionContext
        {
            public MockHubConnectionContext(ConnectionContext connectionContext, HubConnectionContextOptions contextOptions, ILoggerFactory loggerFactory)
                : base(connectionContext, contextOptions, loggerFactory)
            {
            }

            public override ValueTask WriteAsync(HubMessage message, CancellationToken cancellationToken = default)
            {
                throw new Exception();
            }
        }
    }
}
