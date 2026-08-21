using eventstore.Models;
using System.Collections.Specialized;

namespace eventstore.Data
{
	public interface IDriverRepository
	{
		public Driver Load(string driverId);
		public Task<Driver> StartedTheRace(string driverId, string raceId);
		public Driver FinishedRace(string driverId, int position);
		public Driver Overtook(string driverFront, string driverBehind);
		public Driver Crashed(string driverId);
		public Driver Disqualified(string driverId, string reason); // wait for implementation
		public Driver Pitted(string driverId, char tyreType);

	}
}
