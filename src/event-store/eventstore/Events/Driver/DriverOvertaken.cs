namespace eventstore.Events.Driver
{
	public class DriverOvertaken
	{
		public string SubjectDriverId { get; set; } // gets overtaken
		public string TargetDriverId { get; set; }  // overtakes
	}
}
