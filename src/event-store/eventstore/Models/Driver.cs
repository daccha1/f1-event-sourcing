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
		public string DriverId { get; set; }
		public string CurrentRaceId { get; set; }
		public string Name { get; set; }
		public string CurrentTeam { get; set; }
		public int PointsAwarded { get; set; }

		public int CurrentPosition { get; set; }
		public TyreType CurrentTyres { get; set; } = TyreType.Medium;
		
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

		/// Overtakes
		public int NumberOfOvertakes { get; set; } = 0;
		public List<string> HasOvertaken { get; set; } = new();
		public List<string> OvertakenBy { get; set; } = new();

		// PITTED
		public int NumberOfPits { get; set; } = 0;

		// DISQUALIFY
		public bool Disqualified { get; set; } = false;
		public string DisqualifyReason { get; set; }

		
		
		public static Driver StartedRace(string driverId, string raceId, string name, string team)
		{
			var driver = new Driver();

			var evt = new StartedRace()
			{
				DriverId = driverId,
				RaceId = raceId,
				Name = name,
				Team = team
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

		public static Driver FinishedRace(Driver d, int finishPosition)
		{
			var evt = new FinishedRace()
			{
				DriverId = d.DriverId,
				RaceId = d.CurrentRaceId,
				FinishedAt = finishPosition
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<FinishedRace>(evt),
				RootId = d.DriverId
			};

			d.RaiseEvent(baseEvt);
			return d;
		}

		public static Driver Overtook(Driver d, string driverUpfrontId)
		{
			var evt = new DriverOvertook()
			{
				SubjectDriverId = d.DriverId,
				TargetDriverId = driverUpfrontId
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<DriverOvertook>(evt),
				RootId = d.DriverId
			};

			d.RaiseEvent(baseEvt);
			return d;
		}

		public static Driver GotOvertaken(Driver d, string driverOvertakerId)
		{
			var evt = new DriverOvertaken()
			{
				SubjectDriverId = d.DriverId,
				TargetDriverId = driverOvertakerId
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<DriverOvertaken>(evt),
				RootId = d.DriverId
			};

			d.RaiseEvent(baseEvt);
			return d;
		}

		private TyreType ResolveTyreType(char type)
		{
			if (type == 'S') return TyreType.Soft;
			if (type == 'M') return TyreType.Medium;
			if (type == 'H') return TyreType.Hard;
			if (type == 'W') return TyreType.Wet;
			return TyreType.Soft;
		}

		public static Driver Pitted(Driver d, char tyreType)
		{
			var evt = new Pitted()
			{
				DriverId = d.DriverId,
				TyreType = tyreType
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<Pitted>(evt),
				RootId = d.DriverId
			};

			d.RaiseEvent(baseEvt);
			return d;
		}

		public static Driver Crashed(Driver d, DateTime CrashedAt)
		{
			var evt = new Crashed()
			{
				DriverId = d.DriverId,
				OccurredAt = CrashedAt
			};

			var baseEvt = new Event()
			{
				EventType = "Crashed",
				Payload = Event.Serialize<Crashed>(evt),
				RootId = d.DriverId
			};

			d.RaiseEvent(baseEvt);
			return d;
		}

		public static Driver GotDisqualified(Driver d, string reason)
		{
			var evt = new Disqualified()
			{
				DriverId = d.DriverId,
				Reason = reason
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<Disqualified>(evt),
				RootId = d.DriverId
			};

			d.RaiseEvent(baseEvt);
			return d;
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
					StartedAt = DateTime.UtcNow; // needs to be in payload
					Name = startedRace.Name;
					CurrentTeam = startedRace.Team;
					break;
				case "FinishedRace":
					var finishedRace = Event.Deserialize<FinishedRace>(baseEvt.Payload);
					HasFinished = true;
					FinishedAtPosition = finishedRace.FinishedAt;
					FinishedAtTime = DateTime.UtcNow; // needs to be in payload
					break;
				case "DriverOvertook":
					var overtakenEvent = Event.Deserialize<DriverOvertook>(baseEvt.Payload);
					CurrentPosition = CurrentPosition + 1;
					NumberOfOvertakes++;
					HasOvertaken.Add(overtakenEvent.TargetDriverId);
					break;
				case "DriverOvertaken":
					var gotOvertaken = Event.Deserialize<DriverOvertaken>(baseEvt.Payload);
					CurrentPosition = CurrentPosition - 1;
					OvertakenBy.Add(gotOvertaken.TargetDriverId);
					break;
				case "Disqualified":
					var driverDisqualified= Event.Deserialize<Disqualified>(baseEvt.Payload);
					Disqualified = true;
					DisqualifyReason = driverDisqualified.Reason;
					break;
				case "Pitted":
					var driverPitted = Event.Deserialize<Pitted>(baseEvt.Payload);
					NumberOfPits++;
					CurrentTyres = (TyreType) ResolveTyreType(driverPitted.TyreType);
					break;
				case "Crashed":
					var driverCrashed = Event.Deserialize<Crashed>(baseEvt.Payload);
					HasCrashed = true;
					CrashedAt = driverCrashed.OccurredAt;
					CurrentPosition = -1;
					PointsAwarded = 0;
					
					break;
				default:
					throw new Exception("Unrecognized event type.");
			}
		}
	}
}
