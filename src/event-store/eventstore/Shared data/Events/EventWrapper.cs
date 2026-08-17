namespace eventstore.Shared_data.Events
{
	public class EventWrapper
	{
		public string CorrelationId { get; set; }
		public string Payload { get; set; }
		public string EventType { get; set; }
		public DateTime OccuredAt { get; set; }
	}
}
