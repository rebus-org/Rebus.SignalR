using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Transport;
// ReSharper disable AccessToDisposedClosure

namespace Rebus.Internals
{
    public static class RebusExtensions
    {
        internal const string SpecialCorrelationIdPrefix = "multicast-req-";

        public static async Task<TReply> PublishRequest<TReply>(this IBus bus, object request, CancellationToken cancellationToken = default)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var correlationId = $"{SpecialCorrelationIdPrefix}{Guid.NewGuid():N}";

            var replyHandler = MulticastRequestExtensions.GetReplyHandler<TReply>(correlationId);

            await using var timeoutRegistration = cancellationToken.Register(() => replyHandler.SetCanceled(cancellationToken));

            // be sure event is published immediately
            using var _ = new RebusTransactionScopeSuppressor();

            await bus.Publish(request, new Dictionary<string, string>
            {
                [Headers.CorrelationId] = correlationId
            });

            return await replyHandler.Task;
        }
    }
}