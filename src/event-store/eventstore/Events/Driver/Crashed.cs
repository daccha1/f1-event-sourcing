namespace eventstore.Events.Driver
{
	public class Crashed
	{
		public string DriverId { get; set; }
		public DateTime OccurredAt { get; set; }
	}
}
