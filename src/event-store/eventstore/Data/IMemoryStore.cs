using eventstore.Events;
using eventstore.Models;

namespace eventstore.Data
{
	public interface IMemoryStore
	{
		public Race FinishRace(string id);
		public Race Load(string raceId);
		public Race AddNew(string id, string ctr, string gp, int laps);
		public Race StopRace(string id, int lap);
	}

}
