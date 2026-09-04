using eventstore.Data;
using eventstore.Events;
using eventstore.Models;
using eventstore.Services.Caching;

namespace eventstore.Repositories
{
	public class DriversRepository : IDriverRepository
	{
		private EventStoreDbContext _context;
		private readonly ICacheVersionProvider _cacheVersion;

		public DriversRepository(EventStoreDbContext db, ICacheVersionProvider cacheVersion)
		{
			_context = db;
			_cacheVersion = cacheVersion;
		}

		/// <summary>
		/// Appends events and retires any cached read model they invalidate. Every write path
		/// goes through here, so the cache cannot be left holding a stale projection.
		/// </summary>
		private async Task AppendAsync(IReadOnlyList<Event> events)
		{
			_context.Events.AddRange(events);
			await _context.SaveChangesAsync();
			await _cacheVersion.NotifyEventsAppendedAsync(events);
		}

		public async Task<Driver> Crashed(string driverId, DateTime OccurredAt)
		{
			Driver d = await Load(driverId);
			d = Driver.Crashed(d, OccurredAt);
			await AppendAsync(d.DequeueUnsavedEvents());
			return d;
		}

		public async Task<Driver> Overtook(string driverFront, string driverBehind)
		{
			Driver d1 = await Load(driverFront);
			Driver d2 = await Load(driverBehind);

			d2 = Driver.Overtook(d2, driverFront);
			d1 = Driver.GotOvertaken(d1, driverBehind);

			var appended = new List<Event>();
			appended.AddRange(d2.DequeueUnsavedEvents());
			appended.AddRange(d1.DequeueUnsavedEvents());

			await AppendAsync(appended);
			return d2;
		}

		public async Task<Driver> StartedTheRace(string driverId, string raceId, string name, string team)
		{
			Driver d = Driver.StartedRace(driverId, raceId, name, team);
			await AppendAsync(d.DequeueUnsavedEvents());
			return d;
		}

		public async Task<Driver> Load(string driverId)
		{
			var evts = _context.Events.Where(e => e.RootId == driverId).ToList();
			if (evts.Count == 0)
			{
				throw new Exception("List is empty");
			}

			Driver d = new();
			d.LoadEvents(evts);
			return d;
		}

		public async Task<Driver> Pitted(string driverId, char tyreType)
		{
			Driver d = await Load(driverId);
			d = Driver.Pitted(d, tyreType);
			await AppendAsync(d.DequeueUnsavedEvents());
			return d;
		}

		public async Task<Driver> Disqualified(string driverId, string reason)
		{
			Driver d = await Load(driverId);
			d = Driver.GotDisqualified(d, reason);
			await AppendAsync(d.DequeueUnsavedEvents());
			return d;
		}

		public async Task<Driver> FinishedRace(string driverId, int position)
		{
			var driver = await Load(driverId);
			driver = Driver.FinishedRace(driver, position);
			await AppendAsync(driver.DequeueUnsavedEvents());
			return driver;
		}
	}
}
