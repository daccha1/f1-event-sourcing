using eventstore.Controllers;
using eventstore.Data;
using eventstore.Events;
using eventstore.Events.Driver;
using Microsoft.EntityFrameworkCore;

namespace eventstore.Services.Statistics
{
	/// <summary>
	/// Rebuilds championship standings by replaying the event stream. This is the expensive
	/// read path the cache sits in front of, and the only place the point tables are computed,
	/// so the cached and uncached endpoints can never drift apart.
	/// </summary>
	public interface IStandingsProjection
	{
		Task<List<ControllerHelper.Driver>> BuildDriverStandingsAsync(CancellationToken cancellationToken = default);

		Task<List<ControllerHelper.ConstructorStanding>> BuildConstructorStandingsAsync(CancellationToken cancellationToken = default);
	}

	public class StandingsProjection : IStandingsProjection
	{
		private readonly EventStoreDbContext _context;

		public StandingsProjection(EventStoreDbContext context)
		{
			_context = context;
		}

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

		public async Task<List<ControllerHelper.Driver>> BuildDriverStandingsAsync(CancellationToken cancellationToken = default)
		{
			List<ControllerHelper.Driver> drivers = new();
			List<ControllerHelper.DriverRaceIdentity> driverIdentities = new();

			var startedEvents = await _context.Events
				.Where(evt => evt.EventType == "StartedRace")
				.OrderBy(evt => evt.Id)
				.ToListAsync(cancellationToken);

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

			var finishedEvents = await _context.Events
				.Where(evt => evt.EventType == "FinishedRace")
				.OrderBy(evt => evt.Id)
				.ToListAsync(cancellationToken);

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

			return drivers;
		}

		public async Task<List<ControllerHelper.ConstructorStanding>> BuildConstructorStandingsAsync(CancellationToken cancellationToken = default)
		{
			List<ControllerHelper.Driver> drivers = new();
			List<ControllerHelper.DriverRaceIdentity> driverIdentities = new();

			var startedEvents = await _context.Events
				.Where(evt => evt.EventType == "StartedRace")
				.OrderBy(evt => evt.Id)
				.ToListAsync(cancellationToken);

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

			var finishedEvents = await _context.Events
				.Where(evt => evt.EventType == "FinishedRace")
				.OrderBy(evt => evt.Id)
				.ToListAsync(cancellationToken);

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

			return constructors;
		}
	}
}
