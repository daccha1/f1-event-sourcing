using eventstore.Services.RabbitMQ;

namespace eventstore.HostedServices
{
	public class MQBackgroundService : BackgroundService
	{
		private IMQClient mqClient;
		public MQBackgroundService(IMQClient rmqClient)
		{
			mqClient = rmqClient;
		}
		protected async override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await mqClient.StartClient();

			await mqClient.StartListening();

			await Task.Delay(Timeout.Infinite, stoppingToken);

		}
	}
}
