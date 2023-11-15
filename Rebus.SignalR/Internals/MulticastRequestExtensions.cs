using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;

namespace Rebus.Internals
{
    public static class MulticastRequestExtensions
    {
        static readonly ConcurrentDictionary<string, Action<object>> ReplyHandlers = new();

        public static void EnableMulticastRequestReply(this OptionsConfigurer configurer)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            configurer
                .Decorate<IPipeline>(c =>
                {
                    var pipeline = c.Get<IPipeline>();
                    var rebusLoggerFactory = c.Get<IRebusLoggerFactory>();
                    var step = new DispatchIncomingRepliesStep(rebusLoggerFactory);

                    return new PipelineStepInjector(pipeline)
                        .OnReceive(step, PipelineRelativePosition.Before, typeof(DispatchIncomingMessageStep));
                });
        }

        class DispatchIncomingRepliesStep : IIncomingStep
        {
            readonly ILog _logger;

            public DispatchIncomingRepliesStep(IRebusLoggerFactory rebusLoggerFactory)
            {
                if (rebusLoggerFactory == null) throw new ArgumentNullException(nameof(rebusLoggerFactory));
                _logger = rebusLoggerFactory.GetLogger<DispatchIncomingRepliesStep>();
            }

            public async Task Process(IncomingStepContext context, Func<Task> next)
            {
                var message = context.Load<Message>();

                var canBeReply = message.Headers.TryGetValue(Headers.Intent, out var intent) && intent == "p2p";

                if (canBeReply)
                {
                    if (message.Headers.TryGetValue(Headers.CorrelationId, out var correlationId)
                        && correlationId.StartsWith(RebusExtensions.SpecialCorrelationIdPrefix))
                    {
                        if (!ReplyHandlers.TryGetValue(correlationId, out var handler))
                        {
                            _logger.Warn(
                                "Received reply with correlation ID {correlationId} which could not be correlated with a handler",
                                correlationId);
                            return;
                        }

                        handler(message.Body);
                        return;
                    }
                }

                await next();
            }
        }

        internal static TaskCompletionSource<TReply> GetReplyHandler<TReply>(string correlationId)
        {
            var taskCompletionSource = new TaskCompletionSource<TReply>();

            ReplyHandlers[correlationId] = reply =>
            {
                try
                {
                    SetResult(reply);
                }
                catch (Exception exception)
                {
                    taskCompletionSource.SetException(exception);
                }
            };

            return taskCompletionSource;

            void SetResult(object reply)
            {
                try
                {
                    taskCompletionSource.SetResult((TReply)reply);
                }
                catch (Exception exception)
                {
                    throw new ApplicationException(
                        $"Could not complete TaskCompletionSource<{typeof(TReply).Name}> with reply message {reply}",
                        exception);
                }
            }
        }
    }
}