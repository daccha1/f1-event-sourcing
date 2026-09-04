using eventstore.Data;
using eventstore.Events;
using eventstore.Services.Caching;
using eventstore.Services.Statistics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using System.Diagnostics;

namespace eventstore.Controllers
{
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
	public class StatisticsController(
		IDriverRepository driverRepo,
		IRaceRepository raceRepo,
		EventStoreDbContext context,
		IStandingsProjection standings,
		ICacheVersionProvider cacheVersion,
		HybridCache cache) : ControllerBase
	{
		/// <summary>
		/// Reports how the response was produced, so the cached and uncached endpoints can be
		/// compared from a plain curl without attaching a profiler.
		/// </summary>
		private void ReportTiming(string source, Stopwatch stopwatch, string cacheStatus, long? version = null)
		{
			Response.Headers["X-Data-Source"] = source;
			Response.Headers["X-Cache"] = cacheStatus;
			Response.Headers["X-Elapsed-Ms"] = stopwatch.Elapsed.TotalMilliseconds.ToString("F2");

			if (version.HasValue)
			{
				Response.Headers["X-Cache-Version"] = version.Value.ToString();
			}
		}

		/// <summary>
		/// Returns the current driver championship standings, served through Redis.
		/// </summary>
		[HttpGet("scoreboard")]
		public async Task<ActionResult> GetDriverStatistics(CancellationToken cancellationToken)
		{
			var stopwatch = Stopwatch.StartNew();
			var version = await cacheVersion.GetVersionAsync(CacheScope.Standings.Name, cancellationToken);

			// Set only when the factory actually runs, which is exactly when the cache missed.
			bool rebuilt = false;

			var drivers = await cache.GetOrCreateAsync(
				$"standings:drivers:v{version}",
				async ct =>
				{
					rebuilt = true;
					return await standings.BuildDriverStandingsAsync(ct);
				},
				cancellationToken: cancellationToken);

			stopwatch.Stop();
			ReportTiming("cache", stopwatch, rebuilt ? "MISS" : "HIT", version);

			return Ok(drivers);
		}

		/// <summary>
		/// Returns the driver championship standings rebuilt from the event stream on every
		/// request, bypassing Redis entirely. Kept as the baseline for comparing against
		/// <c>GET /api/statistics/scoreboard</c>.
		/// </summary>
		[HttpGet("scoreboard/no-cache")]
		public async Task<ActionResult> GetDriverStatisticsUncached(CancellationToken cancellationToken)
		{
			var stopwatch = Stopwatch.StartNew();
			var drivers = await standings.BuildDriverStandingsAsync(cancellationToken);
			stopwatch.Stop();

			ReportTiming("database", stopwatch, "BYPASS");

			return Ok(drivers);
		}

		/// <summary>
		/// Returns constructor standings calculated from their drivers' championship points,
		/// served through Redis.
		/// </summary>
		[HttpGet("constructors")]
		public async Task<ActionResult> GetConstructorStandings(CancellationToken cancellationToken)
		{
			var stopwatch = Stopwatch.StartNew();
			var version = await cacheVersion.GetVersionAsync(CacheScope.Standings.Name, cancellationToken);

			bool rebuilt = false;

			var constructors = await cache.GetOrCreateAsync(
				$"standings:constructors:v{version}",
				async ct =>
				{
					rebuilt = true;
					return await standings.BuildConstructorStandingsAsync(ct);
				},
				cancellationToken: cancellationToken);

			stopwatch.Stop();
			ReportTiming("cache", stopwatch, rebuilt ? "MISS" : "HIT", version);

			return Ok(constructors);
		}

		/// <summary>
		/// Returns constructor standings rebuilt from the event stream on every request,
		/// bypassing Redis entirely.
		/// </summary>
		[HttpGet("constructors/no-cache")]
		public async Task<ActionResult> GetConstructorStandingsUncached(CancellationToken cancellationToken)
		{
			var stopwatch = Stopwatch.StartNew();
			var constructors = await standings.BuildConstructorStandingsAsync(cancellationToken);
			stopwatch.Stop();

			ReportTiming("database", stopwatch, "BYPASS");

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
