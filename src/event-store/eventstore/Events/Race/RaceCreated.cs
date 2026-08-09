namespace eventstore.Events.Race
{
	public class RaceCreated
	{
		public string RaceId { get; set; }
		public string GrandPrixName { get; set; }
		public string Country { get; set; }
		public int Laps { get; set; }
	}
}
