using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.SignalR.Contracts;
using Rebus.SignalR.Handlers;

namespace Rebus.SignalR
{
    /// <summary>
    /// Dependency injection extensions
    /// </summary>
	public static class RebusSignalRServerBuilderExtensions
	{
        /// <summary>
        /// Registers the Rebus Hub Lifetime Manager for specified THub
        /// </summary>
        /// <param name="signalRServerBuilder">The SignalR builder abstraction for configuring SignalR servers.</param>
        public static ISignalRServerBuilder AddRebusBackplane<THub>(this ISignalRServerBuilder signalRServerBuilder)
            where THub : Hub
        {
            signalRServerBuilder.Services.AddTransient<IHandleMessages<AddToGroup<THub>>, AddToGroupHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<RemoveFromGroup<THub>>, RemoveFromGroupHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<All<THub>>, AllHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<Connection<THub>>, ConnectionHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<Group<THub>>, GroupHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<User<THub>>, UserHandler<THub>>();

            signalRServerBuilder.Services.AddSingleton<HubLifetimeManager<THub>, RebusHubLifetimeManager<THub>>(sp => 
            {
                var bus = sp.GetService<IBus>();
                var syncBus = bus.Advanced.SyncBus;
                var hubProtocolResolver = sp.GetService<IHubProtocolResolver>();
                var logger = sp.GetService<ILogger<RebusHubLifetimeManager<THub>>>();
                
                syncBus.Subscribe<AddToGroup<THub>>();
                syncBus.Subscribe<RemoveFromGroup<THub>>();
                syncBus.Subscribe<All<THub>>();
                syncBus.Subscribe<Connection<THub>>();
                syncBus.Subscribe<Group<THub>>();
                syncBus.Subscribe<User<THub>>();

                var busLifetimeEvents = sp.GetService<BusLifetimeEvents>();
                busLifetimeEvents.BusDisposing += () =>
                {
                    syncBus.Unsubscribe<AddToGroup<THub>>();
                    syncBus.Unsubscribe<RemoveFromGroup<THub>>();
                    syncBus.Unsubscribe<All<THub>>();
                    syncBus.Unsubscribe<Connection<THub>>();
                    syncBus.Unsubscribe<Group<THub>>();
                    syncBus.Unsubscribe<User<THub>>();
                };

                var rebusHubLifetimeManager = new RebusHubLifetimeManager<THub>(bus, hubProtocolResolver, logger);

                return rebusHubLifetimeManager;
            });

            return signalRServerBuilder;
        }
    }
}
