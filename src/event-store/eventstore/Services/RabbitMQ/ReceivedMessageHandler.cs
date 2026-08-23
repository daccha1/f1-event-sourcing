using eventstore.Controllers;
using eventstore.Data;
using eventstore.Repositories;
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
		private IRaceRepository _raceRepository;
		IServiceScopeFactory scopeFactory;
		public ReceivedMessageHandler(IDriverRepository driverRepo, IRaceRepository raceRepo, IServiceScopeFactory scopeFac)
		{
			_driverRepository = driverRepo;
			_raceRepository = raceRepo;
			scopeFactory = scopeFac;
		}
		public async Task HandleMessage(BasicDeliverEventArgs eventArguments)
		{
			string eventBody = Encoding.UTF8.GetString(eventArguments.Body.Span.ToArray());
			Console.WriteLine(eventBody);
			EventWrapper eventWrapper = JsonSerializer.Deserialize<EventWrapper>(eventBody);

			switch (eventWrapper.EventType)
			{
				case "DriverStartedRace":
					ControllerHelper.Driver_StartRace startRace = JsonSerializer.Deserialize<ControllerHelper.Driver_StartRace>(eventWrapper.Payload);
					await _driverRepository.StartedTheRace(startRace.driverId.ToString(), startRace.raceId.ToString());
					break;
				case "DriverFinishedRace":
					ControllerHelper.Driver_FinishedRace driverFinishedRace = JsonSerializer.Deserialize<ControllerHelper.Driver_FinishedRace>(eventWrapper.Payload);
					await _driverRepository.FinishedRace(driverFinishedRace.driverId.ToString(), driverFinishedRace.position);
					break;
				case "DriverOvertook":
					ControllerHelper.Driver_Overtook driverOvertook = JsonSerializer.Deserialize<ControllerHelper.Driver_Overtook>(eventWrapper.Payload);
					await _driverRepository.Overtook(driverOvertook.driverFront.ToString(), driverOvertook.driverBehind.ToString());
					break;
				// to be added: Disqualified, Crashed, Pitted
				// RACE EVENTS
				case "RaceCreated":
					ControllerHelper.CreateRace createRace = JsonSerializer.Deserialize<ControllerHelper.CreateRace>(eventWrapper.Payload);
					await _raceRepository.AddNew(createRace.raceId.ToString(), createRace.country, createRace.gp, createRace.laps);
					break;
				case "RaceFinished":
					ControllerHelper.FinishRace finishedrace = JsonSerializer.Deserialize<ControllerHelper.FinishRace>(eventWrapper.Payload);
					await _raceRepository.FinishRace(finishedrace.raceId.ToString());
					break;
				default:
					throw new Exception("ReceivedMessageHandler: Unknown event type specified.");
			}


		}
	}
}
