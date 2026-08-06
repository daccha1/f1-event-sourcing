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
		public string Country { get; set; }
		public string GrandPrix { get; set; }
		public int NumberOfLaps { get; set; }
		public int CurrentLap { get; set; }
		public RaceState State { get; set; }


		// here we are putting this domain's events

		public static Race Create(string country, string gp, int laps)
		{
			var race = new Race();

			var evt = new RaceCreated
			{
				Country = country,
				GrandPrixName = gp,
				Laps = laps
			};

			var baseEvt = new Event()
			{
				EventType = evt.GetType().Name,
				Payload = Event.Serialize<RaceCreated>(evt)
			};

			race.Apply(baseEvt);

			Console.WriteLine("Kreiran je novi objekat");
			return race;
		}

		protected override void Apply(Event baseEvt)
		{
			switch (baseEvt.EventType)
			{
				case "RaceCreated":
					var evt = Event.Deserialize<RaceCreated>(baseEvt.Payload);
					Country = evt.Country;
					GrandPrix = evt.GrandPrixName;
					NumberOfLaps = evt.Laps;
					CurrentLap = 1;
					State = RaceState.InProgress;
					break;
				default:
					throw new Exception("Unrecognized event type.");
			}
		}
	}
}
