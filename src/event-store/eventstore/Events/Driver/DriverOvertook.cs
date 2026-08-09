namespace eventstore.Events.Driver
{
	public class DriverOvertook
	{
		public string SubjectDriverId { get; set; } // overtakes
		public string TargetDriverId { get; set; }	// gets overtaken

	}
}
