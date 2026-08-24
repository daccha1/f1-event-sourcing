using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;


namespace eventstore.Services.RabbitMQ
{
	public class MQClient : IMQClient
	{
		ConnectionFactory factory;
		IConnection connection;
		IChannel channel;
		AsyncEventingBasicConsumer consumer;

		// EXCHANGES QUEUES 
		public string simulatorConsumingExchange = "simulatorExchange";
		public string raceEventsQueue = "raceevents";
		public string raceRoutingKey = "raceroute";

		IServiceScopeFactory _scopeFactory;
		IConfiguration _configuration;

		public MQClient(IServiceScopeFactory scopeFactory, IConfiguration configuration)
		{
			_scopeFactory = scopeFactory;
			_configuration = configuration;
		}

		public async Task StartClient()
		{
			try
			{
				factory = new ConnectionFactory
				{
					HostName = _configuration["RabbitMq:Host"] ?? "localhost",
					Port = int.TryParse(_configuration["RabbitMq:Port"], out var port) ? port : 5672,
					UserName = _configuration["RabbitMq:Username"] ?? "guest",
					Password = _configuration["RabbitMq:Password"] ?? "guest"
				};
				connection = await factory.CreateConnectionAsync();
				channel = await connection.CreateChannelAsync();
				consumer = new(channel);

				await EnsureDeclaration();
			}
			catch (Exception)
			{
				throw;
			}
		}

		public async Task EnsureDeclaration()
		{
			if(factory != null && connection != null && channel != null)
			{
				await channel.ExchangeDeclareAsync(
						exchange: simulatorConsumingExchange,
						type: ExchangeType.Direct
					);

				await channel.QueueDeclareAsync(
						queue: raceEventsQueue,
						durable: true,
						passive: false,
						exclusive: false,
						autoDelete: false
					);

				await channel.QueueBindAsync(
						raceEventsQueue,
						simulatorConsumingExchange,
						raceRoutingKey
					);
			}
			else
			{
				throw new Exception("Initial parameters must be configured.");
			}
			
		}

		public async Task StartListening()
		{
			consumer.ReceivedAsync += async (_, ea) =>
			{
				
					// napraviti scope, u scopeu pozvati handler, handler resava sam event preko repositoryja
					using (var scope = _scopeFactory.CreateScope())
					{
						var eventsHandler = scope.ServiceProvider.GetRequiredService<IReceivedMessageHandler>();

						try
						{
							await eventsHandler.HandleMessage(ea);
							Debug.WriteLine("Poruka je obradjena?");
							await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: true);
						}
						catch (Exception ex)
						{
							Console.WriteLine("Greska pri obradi poruke " + ex.Message);
							await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: true, requeue: true);
						}

					}
			};

			await channel.BasicConsumeAsync(
					queue: raceEventsQueue,
					autoAck: false,
					consumer: consumer
			);

			Console.WriteLine("Consumer is listening...");
			Debug.WriteLine(">>>> Consumer is listening...");
		}

		


	}
}
