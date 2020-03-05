using System.Threading.Tasks;

namespace Rebus.SignalR.Samples.Hubs
{
	public interface IChatClient
	{
		Task Send(string message);
	}
}
