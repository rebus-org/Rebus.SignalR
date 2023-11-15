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

        public static async Task<TReply> PublishRequestNew<TReply>(this IBus bus, object request)
        {
            if (bus == null) throw new ArgumentNullException(nameof(bus));
            if (request == null) throw new ArgumentNullException(nameof(request));

            var correlationId = $"{SpecialCorrelationIdPrefix}{Guid.NewGuid():N}";

            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var replyHandler = MulticastRequestExtensions.GetReplyHandler<TReply>(correlationId);

            await using var timeoutRegistration = timeout.Token.Register(() => replyHandler.SetCanceled(timeout.Token));

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