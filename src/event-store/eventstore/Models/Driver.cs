using eventstore.Events;
using eventstore.Events.Driver;

namespace eventstore.Models
{
	public enum TyreType
	{
		Soft,
		Medium,
		Hard,
		Wet
	}
	public class Driver : DomainRoot
	{
		public int Id { get; set; }
		public string DriverId { get; set; }
		public string CurrentRaceId { get; set; }

		public int CurrentPosition { get; set; }
		public TyreType CurrentTyres = TyreType.Medium;
		
		// START
		public int StartingPosition { get; set; }
		public bool HasStarted { get; set; } = false;
		public DateTime StartedAt { get; set; }

		// FINISHING
		public int FinishedAtPosition { get; set; }
		public DateTime FinishedAtTime { get; set; }
		public bool HasFinished { get; set; } = false;

		// CRASHES
		public bool HasCrashed { get; set; } = false;
		public DateTime CrashedAt { get; set; }

		/// Overtaks
		public int NumberOfOvertakes { get; set; } = 0;

		// PITTED
		public int NumberOfPits { get; set; } = 0;

		// DISQUALIFY
		public bool Disqualified { get; set; } = false;
		public string DisqualifyReason { get; set; }

		//public List<string> HasOvertaken { get; set; }

		public static Driver StartedRace(string driverId, string raceId)
		{
			var driver = new Driver();
			
			var evt = new StartedRace()
			{
				DriverId = driverId,
				RaceId = raceId
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<StartedRace>(evt),
				RootId = driverId
			};

			driver.RaiseEvent(baseEvt);
			return driver;

		}

		protected override void Apply(Event baseEvt)
		{
			switch (baseEvt.EventType)
			{
				case "StartedRace":
					var startedRace = Event.Deserialize<StartedRace>(baseEvt.Payload);
					DriverId = startedRace.DriverId;
					CurrentRaceId = startedRace.RaceId;
					CurrentPosition = 1;
					StartingPosition = 1;
					HasStarted = true;
					StartedAt = DateTime.UtcNow;
					break;
				default:
					throw new Exception("Unrecognized event type.");
			}
		}
	}
}
