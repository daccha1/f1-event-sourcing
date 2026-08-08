using eventstore.Events;
using eventstore.Events.Race;

namespace eventstore.Models
{
	public enum RaceState
	{
		NotStarted,
		InProgress,
		Finished,
		Cancelled
	}

	public class Race : DomainRoot
	{
		public int Id { get; set; }
		public string RaceId { get; set; }
		public int Year { get; set; }
		public string Country { get; set; }
		public string GrandPrix { get; set; }
		public int NumberOfLaps { get; set; }
		public int CurrentLap { get; set; }
		public RaceState State { get; set; }


		// here we are putting this domain's events

		public static Race Create(string id, string country, string gp, int laps)
		{
			var race = new Race();

			var evt = new RaceCreated
			{
				RaceId = id,
				Country = country,
				GrandPrixName = gp,
				Laps = laps
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<RaceCreated>(evt),
				RootId = evt.RaceId
			};

			race.RaiseEvent(baseEvt);

			return race;
		}

		public static Race FinishRace(Race race)
		{
			var evt = new RaceFinished()
			{
				RaceId = race.RaceId,
				Lap = race.NumberOfLaps
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<RaceFinished>(evt),
				RootId = evt.RaceId
			};

			race.RaiseEvent(baseEvt);
			return race;
		}

		public static Race StopRace(Race race, int lap)
		{
			var evt = new RaceStopped()
			{
				Lap = lap
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<RaceStopped>(evt),
				RootId = race.RaceId
			};

			race.RaiseEvent(baseEvt);
			return race;
		}


		protected override void Apply(Event baseEvt)
		{
			switch (baseEvt.EventType)
			{
				case "RaceCreated":
					var createdEvt = Event.Deserialize<RaceCreated>(baseEvt.Payload);
					RaceId = baseEvt.RootId;
					Country = createdEvt.Country;
					GrandPrix = createdEvt.GrandPrixName;
					NumberOfLaps = createdEvt.Laps;
					CurrentLap = 1;
					State = RaceState.InProgress;
					Year = 2025;
					break;
				case "RaceFinished":
					var finishedEvt = Event.Deserialize<RaceFinished>(baseEvt.Payload);
					CurrentLap = finishedEvt.Lap;
					break;
				case "RaceStopped":
					var stoppedEvt = Event.Deserialize<RaceStopped>(baseEvt.Payload);
					CurrentLap = stoppedEvt.Lap;
					State = RaceState.Cancelled;
					break;
				default:
					throw new Exception("Unrecognized event type.");
			}
		}
	}
}
