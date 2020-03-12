# Rebus.SignalR

[![install from nuget](https://img.shields.io/nuget/v/Rebus.SignalR.svg?style=flat-square)](https://www.nuget.org/packages/Rebus.SignalR)

Rebus-based SignalR backplane... useful, if you are using Rebus already and/or would like to leverage Rebus' integration with various transports not supported by SignalR's own backplane integrations.

How to use
====
Just add AddRebusBackplane&lt;THub&gt;() method call for each hub, that you're going to use with Rebus.SignalR backplane.
```csharp
services.AddSignalR()
    .AddRebusBackplane<ChatHub>();
```
 
Configure Rebus IBus as usual, but keep in mind several things:
* Use an auto generated unique name for the input queue, that will be used as a backplane. In case of Rebus.RabbitMq you should probably configure the input queue as auto-delete. 
* Enable Rebus.Async with EnableSynchronousRequestReply() method call, if you're going to use AddToGroupAsync() and RemoveFromGroupAsync() in SignalR hubs.
* If you're using a decentralized subscription storage, for example Sql Server configured with isCentralized option set to false (by default), you have to map Rebus.SignalR backplane commands for each hub to your queue. To do that, just call MapSignalRCommands&lt;THub&gt;() extension method for type-based router: 

Sample application 1 (RabbitMq is used as a transport with the centralized subscription storage)
====
```csharp
private static string GenerateTransientQueueName(string inputQueueName)
{
    return $"{inputQueueName}-{Environment.MachineName}-{Guid.NewGuid()}";
}

public void ConfigureServices(IServiceCollection services)
{
    services.AddSignalR()
        .AddRebusBackplane<ChatHub>();

    services.AddRebus(configure => configure
        .Transport(x =>
        {
            x.UseRabbitMq("amqp://guest:guest@localhost:5672", GenerateTransientQueueName("Rebus.SignalR"))
            .InputQueueOptions(o =>
            {
                o.SetAutoDelete(true);
                o.SetDurable(false);
            });
        })
        .Options(o => o.EnableSynchronousRequestReply())
        .Routing(r => r.TypeBased()));
}
```

Sample application 2 (SQL Server is used as a transport with the decentralized subscription storage)
====
```csharp
public void ConfigureServices(IServiceCollection services)
{
	services.AddSignalR()
        .AddRebusBackplane<ChatHub>();

	var queueName = GenerateTransientQueueName("Rebus.SignalR");
	services.AddRebus(configure => configure
		.Transport(x => x.UseSqlServer(SignalRBackplaneConnectionString, queueName, isCentralized: false))
        .Options(o => o.EnableSynchronousRequestReply())
        .Routing(r => r.TypeBased()
            .MapSignalRCommands<ChatHub>(queueName))
		.Subscriptions(s => s.StoreInSqlServer(SignalRBackplaneConnectionString, "Subscriptions", false)));                    
}
```

![](https://raw.githubusercontent.com/rebus-org/Rebus/master/artwork/little_rebusbus2_copy-200x200.png)

---