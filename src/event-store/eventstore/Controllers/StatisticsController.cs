using eventstore.Data;
using eventstore.Events;
using eventstore.Events.Driver;
using eventstore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Text.Json;

namespace eventstore.Controllers
{
		/// <summary>
		/// Returns the current driver championship standings calculated from all completed races.
		/// </summary>
	public partial class ControllerHelper
	{
		public record Driver(string rootId, string name, string team)
		{
			public int points { get; set; } = 0;
		}

		public record DriverFinishedEvent(string DriverId, string RaceId, int FinishedAt);
			
		public record DriverRaceIdentity(string raceDriverId, Driver driver);

		public record ConstructorStanding(string team, int points, List<Driver> drivers);

		public record TestDriver(string name, string team, int points)
		{
			public int points { get; set; } = 0;
			public string rootId { get; set; }
		}
	}

	[ApiController]
	[Route("api/[controller]")]
	public class StatisticsController(IDriverRepository driverRepo, IRaceRepository raceRepo, EventStoreDbContext context) : ControllerBase
	{

		private static int HandlePointDistribution(int position)
		{
			return position switch
			{
				1 => 25,
				2 => 18,
				3 => 15,
				4 => 12,
				5 => 10,
				6 => 8,
				7 => 6,
				8 => 4,
				9 => 2,
				10 => 1,
				_ => 0
			};
		}


		//[HttpGet("scoreboard")]
		//public async Task<ActionResult> GetDriverStatistics()
		//{
		//	List<ControllerHelper.Driver> drivers = new();

		//	var raceList = await context.Events.Where(evt => evt.EventType == "RaceCreated").ToListAsync();

		//	foreach (var race in raceList)
		//	{
		//		var raceId = race.RootId;

		//		var driversStarted = await context.Events.Where(evt => evt.EventType == "StartedRace").ToListAsync();

		//		foreach (var d in driversStarted)
		//		{
		//			ControllerHelper.Driver_StartRace evt = Event.Deserialize<ControllerHelper.Driver_StartRace>(d.Payload);
		//			ControllerHelper.Driver driver = new(d.RootId, evt.name, evt.team);

		//			if (drivers.Any(dr => dr.name == driver.name && dr.team == driver.team))
		//			{
		//				continue;
		//			}
		//			else
		//			{
		//				drivers.Add(driver);
		//			}
		//		}

		//		foreach (var driver in drivers)
		//		{
		//			var finishedEvts = await context.Events.Where(e => e.RootId == driver.rootId && e.EventType == "FinishedRace").ToListAsync();

		//			foreach (var stat in finishedEvts)
		//			{
		//				var finishedStat = Event.Deserialize<FinishedRace>(stat.Payload);
		//				driver.points += HandlePointDistribution(finishedStat.FinishedAt);
		//			}
		//		}

		//	}

		//	return Ok(drivers);
		//}

		//[HttpGet("scores")]
		//public async Task<ActionResult> GetDrivers()
		//{
		//	List<ControllerHelper.TestDriver> mainDrivers = new();

		//	var races = await context.Events.Where(evt => evt.EventType == "RaceCreated").ToListAsync();

		//	foreach(var race in races)
		//	{
		//		var raceId = race.RootId;

		//		var driversInRace = await context.Events.Where(evt => evt.EventType == "StartedRace").ToListAsync();

		//		//.Select(evt => JsonSerializer.Deserialize<ControllerHelper.Driver_StartRace>(evt.Payload)).Where(evt => evt.raceId.ToString() == raceId).ToListAsync();

		//		List<ControllerHelper.Driver_StartRace> driversInRaceEvts = new();

		//		foreach(var driver in driversInRace)
		//		{
		//			var evt = JsonSerializer.Deserialize<ControllerHelper.Driver_StartRace>(driver.Payload);

		//		}


		//		foreach (var driver in driversInRace)
		//		{
		//			ControllerHelper.TestDriver td = new(driver.name, driver.team, 0);
		//			td.rootId = driver.driverId.ToString();

		//			if(mainDrivers.Any(driver => driver.name == td.name && driver.team == td.team))
		//			{
		//				mainDrivers.Where(driver => driver.name == td.name && driver.team == td.team).FirstOrDefault().rootId = td.rootId;
		//			}
		//			else
		//			{
		//				mainDrivers.Add(td);
		//			}

		//			var finishStats = await context.Events.Where(evt => evt.EventType == "FinishedRace").Select(evt => JsonSerializer.Deserialize<ControllerHelper.Driver_FinishedRace>(evt.Payload)).Where(evt => evt.driverId.ToString() == td.rootId).FirstOrDefaultAsync();


		//			mainDrivers.Where(d => d.rootId == td.rootId).FirstOrDefault().points += HandlePointDistribution(finishStats.position);

		//			return Ok(mainDrivers);
		//		}

		//	}



		//	return Ok();
		//}

		[HttpGet("scoreboard")]
		public async Task<ActionResult> GetDriverStatistics()
		{
			List<ControllerHelper.Driver> drivers = new();
			List<ControllerHelper.DriverRaceIdentity> driverIdentities = new();

			var startedEvents = await context.Events
				.Where(evt => evt.EventType == "StartedRace")
				.OrderBy(evt => evt.Id)
				.ToListAsync();

			foreach (var startedEvent in startedEvents)
			{
				var startedDriver =
					Event.Deserialize<ControllerHelper.Driver_StartRace>(
						startedEvent.Payload
					);

				ControllerHelper.Driver? existingDriver = null;

				for (int i = 0; i < drivers.Count; i++)
				{
					if (drivers[i].name == startedDriver.name &&
						drivers[i].team == startedDriver.team)
					{
						existingDriver = drivers[i];
						break;
					}
				}

				if (existingDriver == null)
				{
					existingDriver = new ControllerHelper.Driver(
						startedDriver.driverId.ToString(),
						startedDriver.name,
						startedDriver.team
					);

					drivers.Add(existingDriver);
				}

				driverIdentities.Add(
					new ControllerHelper.DriverRaceIdentity(
						startedEvent.RootId,
						existingDriver
					)
				);
			}

			var finishedEvents = await context.Events
				.Where(evt => evt.EventType == "FinishedRace")
				.OrderBy(evt => evt.Id)
				.ToListAsync();

			foreach (var finishedEvent in finishedEvents)
			{
				ControllerHelper.Driver? driver = null;

				for (int i = 0; i < driverIdentities.Count; i++)
				{
					if (driverIdentities[i].raceDriverId == finishedEvent.RootId)
					{
						driver = driverIdentities[i].driver;
						break;
					}
				}

				if (driver == null)
				{
					continue;
				}

				var finishedRace =
					Event.Deserialize<FinishedRace>(finishedEvent.Payload);

				driver.points += HandlePointDistribution(
					finishedRace.FinishedAt
				);
			}

			drivers.Sort((first, second) =>
			{
				return second.points.CompareTo(first.points);
			});

			return Ok(drivers);
		}

		/// <summary>
		/// Returns constructor standings calculated from their drivers' championship points.
		/// </summary>
		[HttpGet("constructors")]
		public async Task<ActionResult> GetConstructorStandings()
		{
			List<ControllerHelper.Driver> drivers = new();
			List<ControllerHelper.DriverRaceIdentity> driverIdentities = new();

			var startedEvents = await context.Events
				.Where(evt => evt.EventType == "StartedRace")
				.OrderBy(evt => evt.Id)
				.ToListAsync();

			foreach (var startedEvent in startedEvents)
			{
				var startedDriver =
					Event.Deserialize<ControllerHelper.Driver_StartRace>(
						startedEvent.Payload
					);

				ControllerHelper.Driver? existingDriver = null;

				for (int i = 0; i < drivers.Count; i++)
				{
					if (drivers[i].name == startedDriver.name)
					{
						existingDriver = drivers[i];
						break;
					}
				}

				if (existingDriver == null)
				{
					existingDriver = new ControllerHelper.Driver(
						startedEvent.RootId,
						startedDriver.name,
						startedDriver.team
					);

					drivers.Add(existingDriver);
				}

				driverIdentities.Add(
					new ControllerHelper.DriverRaceIdentity(
						startedEvent.RootId,
						existingDriver
					)
				);
			}

			var finishedEvents = await context.Events
				.Where(evt => evt.EventType == "FinishedRace")
				.OrderBy(evt => evt.Id)
				.ToListAsync();

			foreach (var finishedEvent in finishedEvents)
			{
				ControllerHelper.Driver? driver = null;

				for (int i = 0; i < driverIdentities.Count; i++)
				{
					if (driverIdentities[i].raceDriverId == finishedEvent.RootId)
					{
						driver = driverIdentities[i].driver;
						break;
					}
				}

				if (driver == null)
				{
					continue;
				}

				var finishedRace =
					Event.Deserialize<FinishedRace>(finishedEvent.Payload);

				driver.points += HandlePointDistribution(
					finishedRace.FinishedAt
				);
			}

			List<ControllerHelper.ConstructorStanding> constructors = new();

			for (int i = 0; i < drivers.Count; i++)
			{
				ControllerHelper.Driver driver = drivers[i];

				ControllerHelper.ConstructorStanding? existingConstructor = null;

				for (int j = 0; j < constructors.Count; j++)
				{
					if (constructors[j].team == driver.team)
					{
						existingConstructor = constructors[j];
						break;
					}
				}

				if (existingConstructor == null)
				{
					existingConstructor = new ControllerHelper.ConstructorStanding(
						driver.team,
						0,
						new List<ControllerHelper.Driver>()
					);

					constructors.Add(existingConstructor);
				}

				existingConstructor.drivers.Add(driver);
			}

			for (int i = 0; i < constructors.Count; i++)
			{
				int totalPoints = 0;

				for (int j = 0; j < constructors[i].drivers.Count; j++)
				{
					totalPoints += constructors[i].drivers[j].points;
				}

				constructors[i] = constructors[i] with
				{
					points = totalPoints
				};
			}

			constructors.Sort((first, second) =>
			{
				return second.points.CompareTo(first.points);
			});

			return Ok(constructors);
		}

		/// <summary>
		/// Returns identifiers for all created races.
		/// </summary>
		[HttpGet("races")]
		public async Task<ActionResult> GetAllRaces()
		{
			var racesBaseEvent = await context.Events.Where(evt => evt.EventType == "RaceCreated").ToListAsync();

			var races = new List<ControllerHelper.CreateRace>();

			foreach(var race in racesBaseEvent)
			{
				races.Add(Event.Deserialize<ControllerHelper.CreateRace>(race.Payload));
			}

			return Ok(races.Select(race => race.raceId));
		}

		/// <summary>
		/// Returns drivers grouped by race.
		/// </summary>
		[HttpGet("race-drivers")]
		public async Task<ActionResult> GetAllRacesDrivers()
		{
			var racesBaseEvent = await context.Events.Where(evt => evt.EventType == "RaceCreated").ToListAsync();

			var races = new List<ControllerHelper.CreateRace>();
			var allDriversDeserialized = new List<ControllerHelper.Driver_StartRace>();

			var allDrivers = await context.Events.Where(evt => evt.EventType == "StartedRace").ToListAsync();

			foreach(var driver in allDrivers)
			{
				allDriversDeserialized.Add(Event.Deserialize<ControllerHelper.Driver_StartRace>(driver.Payload));
			}

			foreach (var race in racesBaseEvent)
			{
				races.Add(Event.Deserialize<ControllerHelper.CreateRace>(race.Payload));
			}

			Dictionary<string, List<ControllerHelper.Driver_StartRace>> racesDrivers = new();

			foreach(var race in races)
			{
				var drivers = allDriversDeserialized.Where(d => d.raceId == race.raceId).ToList();

				racesDrivers.Add(race.raceId.ToString(), drivers);
		

				
			}

			return Ok(racesDrivers);
		}

		[HttpGet("load-driver/{driverId:guid}")]
		public IActionResult LoadDriver([FromRoute] Guid driverId)
		{
			string str_driver = driverId.ToString();
			var d = driverRepo.Load(str_driver);
			return Ok(d);
		}
	}
}
