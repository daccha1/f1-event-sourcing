using eventstore.Data;
using eventstore.Events;
using eventstore.Models;

namespace eventstore.Repositories
{
	public class RaceMemoryStore : IMemoryStore
	{
		private List<Event> Events = new();

		public Race AddNew(string id, string ctr, string gp, int laps)
		{
			var race = Race.Create(id, ctr, gp, laps);
			return race;
		}
		// 450B11AE-ABB2-442F-89A6-36F09BC86F30 MONACO GUID
	

	}
}
