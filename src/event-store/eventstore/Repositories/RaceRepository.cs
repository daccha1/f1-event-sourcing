using eventstore.Data;
using eventstore.Events;
using eventstore.Models;
using Microsoft.EntityFrameworkCore;

namespace eventstore.Repositories
{
	public class RaceRepository : IRaceRepository
	{
		public List<Event> Events;
		public EventStoreDbContext _context;

		public RaceRepository(EventStoreDbContext db)
		{
			_context = db;
		}
		

		public Race FinishRace(string id)
		{
			var race = Load(id);
			race = Race.FinishRace(race);
			_context.Events.AddRange(race.DequeueUnsavedEvents());
			return race;
		}

		public Race StopRace(string id, int lap)
		{
			var race = Load(id);
			race = Race.StopRace(race, lap);
			_context.Events.AddRange(race.DequeueUnsavedEvents());
			return race;
		}

		public Race Load(string raceId)
		{
			var evts = _context.Events.Where(e => e.RootId == raceId).ToList();
			if(evts.Count == 0)
			{
				throw new Exception("List is empty");
			}
			
			Race r = new();
			r.LoadEvents(evts);
			return r;
		}
		
		public Race AddNew(string id, string ctr, string gp, int laps)
		{
			var race = Race.Create(id, ctr, gp, laps);
			_context.Events.AddRange(race.DequeueUnsavedEvents());
			return race;
		}


	}
}
