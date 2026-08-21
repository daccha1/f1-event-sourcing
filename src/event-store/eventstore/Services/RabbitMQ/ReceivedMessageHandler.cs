using eventstore.Controllers;
using eventstore.Data;
using eventstore.Shared_data.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace eventstore.Services.RabbitMQ
{
	public interface IReceivedMessageHandler
	{
		Task HandleMessage(BasicDeliverEventArgs eventArguments);
	}
	public class ReceivedMessageHandler : IReceivedMessageHandler
	{
		private IDriverRepository _driverRepository;
		IServiceScopeFactory scopeFactory;
		public ReceivedMessageHandler(IDriverRepository driverRepo, IServiceScopeFactory scopeFac)
		{
			_driverRepository = driverRepo;
			scopeFactory = scopeFac;
		}
		public async Task HandleMessage(BasicDeliverEventArgs eventArguments)
		{
			string eventBody = Encoding.UTF8.GetString(eventArguments.Body.Span.ToArray());
			Console.WriteLine(eventBody);
			EventWrapper eventWrapper = JsonSerializer.Deserialize<EventWrapper>(eventBody);

			switch (eventWrapper.EventType)
			{
				case "StartedRace":
					ControllerHelper.Driver_StartRace startRace = JsonSerializer.Deserialize<ControllerHelper.Driver_StartRace>(eventWrapper.Payload);
					await _driverRepository.StartedTheRace(startRace.driverId.ToString(), startRace.raceId.ToString());
					break;
				default:
					throw new Exception();
			}


		}
	}
}
