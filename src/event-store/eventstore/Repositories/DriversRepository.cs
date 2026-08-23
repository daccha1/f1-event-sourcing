using eventstore.Data;
using eventstore.Events;
using eventstore.Models;
using System.Collections.Specialized;
using System.ComponentModel.Design.Serialization;

namespace eventstore.Repositories
{
	public class DriversRepository : IDriverRepository
	{
		private EventStoreDbContext _context;
		private List<Event> Events; 
		public DriversRepository(EventStoreDbContext db)
		{
			_context = db;
		}
		
		public async Task<Driver> Crashed(string driverId, DateTime OccurredAt)
		{
			Driver d = await Load(driverId);
			d = Driver.Crashed(d, OccurredAt);
			await _context.Events.AddRangeAsync(d.DequeueUnsavedEvents());
			await _context.SaveChangesAsync();
			return d;
		}

		public async Task<Driver> Overtook(string driverFront, string driverBehind)
		{
			Driver d1 = await Load(driverFront);
			Driver d2 = await Load(driverBehind);

			d2 = Driver.Overtook(d2, driverFront);
			_context.Events.AddRange(d2.DequeueUnsavedEvents());
			d1 = Driver.GotOvertaken(d1, driverBehind);
			_context.Events.AddRange(d1.DequeueUnsavedEvents());
			
			await _context.SaveChangesAsync();
			return d2;
		}

		public async Task<Driver> StartedTheRace(string driverId, string raceId, string name, string team)
		{
			Driver d = Driver.StartedRace(driverId, raceId, name, team);
			await _context.Events.AddRangeAsync(d.DequeueUnsavedEvents());
			await _context.SaveChangesAsync();
			return d;
		}

		public async Task<Driver> Load(string driverId)
		{
			var evts = _context.Events.Where(e => e.RootId == driverId).ToList();
			if (evts.Count == 0)
			{
				throw new Exception("List is empty");
			}

			Driver d = new();
			d.LoadEvents(evts);
			return d;
		}

		//public Task<Driver> Load(Guid driverId)
		//{
		//	var evts = _context.Events.Where(e => e.RootId == driverId.ToString()).ToList();
		//	if (evts.Count == 0)
		//	{
		//		throw new Exception("List is empty");
		//	}

		//	Driver d = new();
		//	d.LoadEvents(evts);
		//	return d;
		//}

		public async Task<Driver> Pitted(string driverId, char tyreType)
		{
			Driver d = await Load(driverId);
			d = Driver.Pitted(d, tyreType);
			_context.Events.AddRange(d.DequeueUnsavedEvents());
			await _context.SaveChangesAsync();
			return d;
		}

		public async Task<Driver> Disqualified(string driverId, string reason)
		{
			Driver d = await Load(driverId);
			d = Driver.GotDisqualified(d, reason);
			_context.Events.AddRange(d.DequeueUnsavedEvents());
			await _context.SaveChangesAsync();
			return d;

		}

		public async Task<Driver> FinishedRace(string driverId, int position)
		{
			var driver = await Load(driverId);
			driver = Driver.FinishedRace(driver, position);
			_context.Events.AddRange(driver.DequeueUnsavedEvents());
			await _context.SaveChangesAsync();
			return driver;
		}

		
	}
}
