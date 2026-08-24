namespace eventstore.Services.RabbitMQ
{
	public interface IMQClient
	{
		public Task StartClient();
		public Task EnsureDeclaration();
		public Task StartListening();
	}
}
