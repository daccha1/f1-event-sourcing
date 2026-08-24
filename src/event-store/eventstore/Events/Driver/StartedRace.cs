namespace eventstore.Events.Driver
{
	public class StartedRace
	{
		public string DriverId { get; set; }
		public string RaceId { get; set; }
		public string Name { get; set; }
		public string Team { get; set; }

	}
}
