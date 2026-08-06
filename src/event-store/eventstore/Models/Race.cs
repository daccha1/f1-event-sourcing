using eventstore.Events;

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
		public Guid RaceId { get; set; }
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
				RootId = Guid.Parse(evt.RaceId)
			};

			race.RaiseEvent(baseEvt);

			return race;
		}

		protected override void Apply(Event baseEvt)
		{
			switch (baseEvt.EventType)
			{
				case "RaceCreated":
					var evt = Event.Deserialize<RaceCreated>(baseEvt.Payload);
					RaceId = Guid.Parse(evt.RaceId);
					Country = evt.Country;
					GrandPrix = evt.GrandPrixName;
					NumberOfLaps = evt.Laps;
					CurrentLap = 1;
					State = RaceState.InProgress;
					Year = 2025;
					break;
				default:
					throw new Exception("Unrecognized event type.");
			}
		}
	}
}
