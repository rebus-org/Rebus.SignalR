using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;
using Rebus.SignalR.Contracts;
using Rebus.SignalR.Handlers;

namespace Rebus.SignalR
{
    /// <summary>
    /// 
    /// </summary>
	public static class RebusSignalRServerBuilderExtensions
	{
        /// <summary>
        /// Registers the Rebus Hub Lifetime Manager
        /// </summary>
        /// <param name="signalRServerBuilder">The SignalR builder abstraction for configuring SignalR servers.</param>
        public static ISignalRServerBuilder AddRebusBackplane(this ISignalRServerBuilder signalRServerBuilder)
        {
            signalRServerBuilder.Services.AddSingleton(typeof(HubLifetimeManager<>), typeof(RebusHubLifetimeManager<>));

            return signalRServerBuilder;
        }

        /// <summary>
        /// Register Rebus SignalR handlers for backplane commands
        /// </summary>
        /// <typeparam name="THub"></typeparam>
        /// <param name="signalRServerBuilder"></param>
        /// <returns></returns>
        public static ISignalRServerBuilder AddRebusBackplaneHandlers<THub>(this ISignalRServerBuilder signalRServerBuilder)
            where THub : Hub
        {
            signalRServerBuilder.Services.AddTransient<IHandleMessages<AddToGroup<THub>>, AddToGroupHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<RemoveFromGroup<THub>>, RemoveFromGroupHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<All<THub>>, AllHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<Connection<THub>>, ConnectionHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<Group<THub>>, GroupHandler<THub>>();
            signalRServerBuilder.Services.AddTransient<IHandleMessages<User<THub>>, UserHandler<THub>>();

            return signalRServerBuilder;
        }
    }
}
