using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Rebus.SignalR
{
	/// <summary>
	/// An interface through which backplane command handlers work
	/// </summary>
	public interface IRebusHubLifetimeManager
	{
		/// <summary>
		/// This server's name
		/// </summary>
		string ServerName { get; }

		/// <summary>
		/// Connections to this server
		/// </summary>
		HubConnectionStore Connections { get; }

		/// <summary>
		/// Connections grouped by GroupName
		/// </summary>
		ConcurrentDictionary<string, HubConnectionStore> GroupConnections { get; }

		/// <summary>
		/// Connections grouped by UserIdentifier
		/// </summary>
		ConcurrentDictionary<string, HubConnectionStore> UserConnections { get; }

		/// <summary>
		/// Adds a connection to a group locally
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="groupName"></param>
		void AddToGroupLocal(HubConnectionContext connection, string groupName);

		/// <summary>
		/// Removes a connection from a group locally
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="groupName"></param>
		void RemoveFromGroupLocal(HubConnectionContext connection, string groupName);
	}
}
