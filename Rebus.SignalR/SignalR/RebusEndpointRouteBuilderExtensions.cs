using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;
using Rebus.SignalR.Contracts;

namespace Rebus.SignalR
{
	/// <summary>
	/// 
	/// </summary>
	public static class RebusEndpointRouteBuilderExtensions
	{
		/// <summary>
		/// Subscribes to Rebus SignalR backplane commands
		/// </summary>
		/// <param name="endpoints"></param>
		/// <returns></returns>
		public static IEndpointRouteBuilder SubscribeToRebusBackplaneCommands<THub>(this IEndpointRouteBuilder endpoints)
			where THub : Hub
		{
			var bus = endpoints.ServiceProvider.GetService<IBus>();

			bus.Subscribe<AddToGroup<THub>>();
			bus.Subscribe<RemoveFromGroup<THub>>();
			bus.Subscribe<All<THub>>();
			bus.Subscribe<Connection<THub>>();
			bus.Subscribe<Group<THub>>();
			bus.Subscribe<User<THub>>();

			return endpoints;
		}
	}
}
