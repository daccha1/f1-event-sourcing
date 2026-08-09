using eventstore.Data;
using eventstore.Events;
using eventstore.Models;

namespace eventstore.Repositories
{
	public class RaceMemoryStore : IMemoryStore
	{
		public List<Event> Events;
		public MemoryDatabase _db;

		public RaceMemoryStore(MemoryDatabase db)
		{
			_db = db;
			Events = _db.Events;
		}

		public Race FinishRace(string id)
		{
			var race = Load(id);
			race = Race.FinishRace(race);
			Events.AddRange(race.DequeueUnsavedEvents());
			return race;
		}

		public Race StopRace(string id, int lap)
		{
			var race = Load(id);
			race = Race.StopRace(race, lap);
			Events.AddRange(race.DequeueUnsavedEvents());
			return race;
		}

		public Race Load(string raceId)
		{
			var evts = Events.Where(e => e.RootId == raceId).ToList();
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
			Events.AddRange(race.DequeueUnsavedEvents());
			return race;
		}


	}
}
