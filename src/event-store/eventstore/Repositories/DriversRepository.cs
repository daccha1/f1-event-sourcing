using eventstore.Data;
using eventstore.Events;
using eventstore.Models;
using System.Collections.Specialized;
using System.ComponentModel.Design.Serialization;

namespace eventstore.Repositories
{
	public class DriversRepository : IDriverRepository
	{
		private MemoryDatabase _db;
		private List<Event> Events; 
		public DriversRepository(MemoryDatabase db)
		{
			_db = db;
			Events = _db.Events;
		}

		public Driver Crashed(string driverId)
		{
			throw new NotImplementedException();
		}

		public Driver Overtook(string driverFront, string driverBehind)
		{
			Driver d1 = Load(driverFront);
			Driver d2 = Load(driverBehind);

			d2 = Driver.Overtook(d2, driverFront);
			Events.AddRange(d2.DequeueUnsavedEvents());
			d1 = Driver.GotOvertaken(d1, driverBehind);
			Events.AddRange(d1.DequeueUnsavedEvents());

			return d2;
		}

		public Driver StartedTheRace(string driverId, string raceId)
		{
			Driver d = Driver.StartedRace(driverId, raceId);
			Events.AddRange(d.DequeueUnsavedEvents());
			return d;
		}

		public Driver Load(string driverId)
		{
			var evts = Events.Where(e => e.RootId == driverId).ToList();
			if (evts.Count == 0)
			{
				throw new Exception("List is empty");
			}

			Driver d = new();
			d.LoadEvents(evts);
			return d;
		}

		public Driver Pitted(string driverId, char tyreType)
		{
			Driver d = Load(driverId);
			d = Driver.Pitted(d, tyreType);
			Events.AddRange(d.DequeueUnsavedEvents());
			return d;
		}

		public Driver Disqualified(string driverId, string reason)
		{
			Driver d = Load(driverId);
			d = Driver.GotDisqualified(d, reason);
			Events.AddRange(d.DequeueUnsavedEvents());
			return d;

		}

		public Driver FinishedRace(string driverId, int position)
		{
			var driver = Load(driverId);
			driver = Driver.FinishedRace(driver, position);
			Events.AddRange(driver.DequeueUnsavedEvents());
			return driver;
		}
	}
}
