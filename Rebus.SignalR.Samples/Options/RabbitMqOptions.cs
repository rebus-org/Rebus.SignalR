namespace Rebus.SignalR.Samples.Options
{
	/// <summary>
	/// RabbitMq configuration options
	/// </summary>
	public class RabbitMqOptions
	{
		public string Host { get; set; }

		public int Port { get; set; }

		public string User { get; set; }
		
		public string Password { get; set; }
	}
}