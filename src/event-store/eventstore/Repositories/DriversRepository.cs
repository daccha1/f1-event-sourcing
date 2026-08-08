using eventstore.Data;
using eventstore.Models;

namespace eventstore.Repositories
{
	public class DriversRepository : IDriverRepository
	{
		
		public Driver Crashed(string driverId)
		{
			throw new NotImplementedException();
		}

		public Driver Disqualified(string driverId, string reason)
		{
			throw new NotImplementedException();
		}

		public Driver FinishedTheRace(string driverId)
		{
			throw new NotImplementedException();
		}

		public Driver Overtook(string driverFront, string driverBehind)
		{
			throw new NotImplementedException();
		}

		public Driver Pit(string driverId, string tyreType)
		{
			throw new NotImplementedException();
		}

		public Driver StartedTheRace(string driverId, string raceId)
		{
			throw new NotImplementedException();
		}
	}
}
