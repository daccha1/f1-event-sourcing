using eventstore.Data;
using eventstore.Events;
using eventstore.Models;
using eventstore.Services.Caching;
using Microsoft.EntityFrameworkCore;

namespace eventstore.Repositories
{
	public class RaceRepository : IRaceRepository
	{
		public List<Event> Events;
		public EventStoreDbContext _context;
		private readonly ICacheVersionProvider _cacheVersion;

		public RaceRepository(EventStoreDbContext db, ICacheVersionProvider cacheVersion)
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

		public async Task<Race> FinishRace(string id)
		{
			var race = await Load(id);
			race = Race.FinishRace(race);
			await AppendAsync(race.DequeueUnsavedEvents());
			return race;
		}

		public async Task<Race> StopRace(string id, int lap)
		{
			var race = await Load(id);
			race = Race.StopRace(race, lap);
			await AppendAsync(race.DequeueUnsavedEvents());
			return race;
		}

		public async Task<Race> Load(string raceId)
		{
			var evts = await _context.Events.Where(e => e.RootId == raceId).ToListAsync();
			if(evts.Count == 0)
			{
				throw new Exception("List is empty");
			}
			
			Race r = new();
			r.LoadEvents(evts);
			return r;
		}
		
		public async Task<Race> AddNew(string id, string ctr, string gp, int laps)
		{
			var race = Race.Create(id, ctr, gp, laps);
			await AppendAsync(race.DequeueUnsavedEvents());
			return race;
		}
	}
}
