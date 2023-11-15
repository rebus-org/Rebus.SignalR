using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Internals;
using Rebus.Logging;
using Rebus.Tests.Contracts;
using Rebus.Transport.InMem;

namespace Rebus.SignalR.Tests.Circumventions;

[TestFixture]
[Description("Rebus.Async no longer supports 'multicast request/reply', i.e. publishing requests. Therefore, this functionality has been implemented here.")]
public class MulticaseRequestReply : FixtureBase
{
    InMemNetwork _network;

    protected override void SetUp()
    {
        _network = new InMemNetwork();
    }

    [Test]
    public async Task CanDoMulticastRequestReply()
    {
        var publisher = CreateBus("pubber");

        CreateBus(
            inputQueueName: "subber1",
            subscribe: true,
            handlers: a => a.Handle<MulticastRequest>(async (bus, req) =>
            {
                if (req.IntentedRecipient != 1) return; //< not for this one
                await bus.Reply(new SingleReply("subber1"));
            })
        );

        CreateBus(
            inputQueueName: "subber2",
            subscribe: true,
            handlers: a => a.Handle<MulticastRequest>(async (bus, req) =>
            {
                if (req.IntentedRecipient != 2) return; //< not for this one
                await bus.Reply(new SingleReply("subber2"));
            })
        );

        CreateBus(
            inputQueueName: "subber3",
            subscribe: true,
            handlers: a => a.Handle<MulticastRequest>(async (bus, req) =>
            {
                if (req.IntentedRecipient != 3) return; //< not for this one
                await bus.Reply(new SingleReply("subber3"));
            })
        );

        var reply1 = await publisher.PublishRequest<SingleReply>(new MulticastRequest(IntentedRecipient: 1));
        var reply2 = await publisher.PublishRequest<SingleReply>(new MulticastRequest(IntentedRecipient: 2));
        var reply3 = await publisher.PublishRequest<SingleReply>(new MulticastRequest(IntentedRecipient: 3));

        Assert.That(reply1.Replier, Is.EqualTo("subber1"));
        Assert.That(reply2.Replier, Is.EqualTo("subber2"));
        Assert.That(reply3.Replier, Is.EqualTo("subber3"));
    }

    record MulticastRequest(int IntentedRecipient);

    record SingleReply(string Replier);

    IBus CreateBus(string inputQueueName, Action<BuiltinHandlerActivator> handlers = null, bool subscribe = false)
    {
        var activator = Using(new BuiltinHandlerActivator());

        handlers?.Invoke(activator);

        var bus = Configure.With(activator)
            .Logging(l => l.Console(minLevel: LogLevel.Info))
            .Transport(t => t.UseInMemoryTransport(_network, inputQueueName))
            .Options(o => o.EnableMulticastRequestReply())
            .Start();

        if (subscribe)
        {
            bus.Advanced.SyncBus.Subscribe<MulticastRequest>();
        }

        return bus;
    }
}