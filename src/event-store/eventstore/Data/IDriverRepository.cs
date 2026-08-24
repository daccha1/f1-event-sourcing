using eventstore.Models;
using System.Collections.Specialized;

namespace eventstore.Data
{
	public interface IDriverRepository
	{
		public Task<Driver> Load(string driverId);
		//public Task<Driver> Load(Guid driverId);
		public Task<Driver> StartedTheRace(string driverId, string raceId, string name, string team);
		public Task<Driver> FinishedRace(string driverId, int position);
		public Task<Driver> Overtook(string driverFront, string driverBehind);
		public Task<Driver> Crashed(string driverId, DateTime OccurredAt);
		public Task<Driver> Disqualified(string driverId, string reason); // wait for implementation
		public Task<Driver> Pitted(string driverId, char tyreType);

	}
}
