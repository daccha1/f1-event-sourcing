namespace eventstore.Events.Driver
{
	public class FinishedRace
	{
		public string DriverId { get; set; }
		public string RaceId { get; set; }
		public int  FinishedAt { get; set; }
	}
}
