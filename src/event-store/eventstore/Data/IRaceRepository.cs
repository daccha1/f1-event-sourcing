using eventstore.Events;
using eventstore.Models;

namespace eventstore.Data
{
	public interface IRaceRepository
	{
		public Task<Race> FinishRace(string id);
		public Task<Race> Load(string raceId);
		public Task<Race> AddNew(string id, string ctr, string gp, int laps);
		public Task<Race> StopRace(string id, int lap);
	}

}
