using eventstore.Models;
using System.Collections.Specialized;

namespace eventstore.Data
{
	public interface IDriverRepository
	{
		public Driver StartedTheRace(string driverId, string raceId);
		public Driver FinishedTheRace(string driverId);
		public Driver Overtook(string driverFront, string driverBehind);
		public Driver Crashed(string driverId);
		public Driver Disqualified(string driverId, string reason);
		public Driver Pit(string driverId, string tyreType);

	}
}
