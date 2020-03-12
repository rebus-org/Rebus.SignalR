using Microsoft.AspNetCore.SignalR;
using Rebus.Routing.TypeBased;
using Rebus.SignalR.Contracts;

namespace Rebus.Config
{
	/// <summary>
	/// Configuration extensions for TypeBased router
	/// </summary>
	public static class SignalRTypeBasedRouterConfigurationExtensions
	{
		/// <summary>
		/// Map SignalR backplane commands to the destination address
		/// </summary>
		/// <param name="configurationBuilder"></param>
		/// <param name="destinationAddress"></param>
		/// <typeparam name="THub"></typeparam>
		/// <returns></returns>
		public static TypeBasedRouterConfigurationExtensions.TypeBasedRouterConfigurationBuilder MapSignalRCommands<THub>(
				this TypeBasedRouterConfigurationExtensions.TypeBasedRouterConfigurationBuilder configurationBuilder, string destinationAddress)
			where THub: Hub
		{
			configurationBuilder.Map<AddToGroup<THub>>(destinationAddress);
			configurationBuilder.Map<RemoveFromGroup<THub>>(destinationAddress);
			configurationBuilder.Map<All<THub>>(destinationAddress);
			configurationBuilder.Map<Connection<THub>>(destinationAddress);
			configurationBuilder.Map<Group<THub>>(destinationAddress);
			configurationBuilder.Map<User<THub>>(destinationAddress);
			return configurationBuilder;
		}
	}
}