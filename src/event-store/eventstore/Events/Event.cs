using System.Text.Json;

namespace eventstore.Events
{
	public class Event
	{
		public int Id { get; private set; }
		public string RootId { get; set; }
		public DateTime OccuredAt { get; private set; } = DateTime.UtcNow;

		public string EventType { get; set; } // for deserialization
		public string Payload { get; set; }	  // deserialization payload

		public Event()
		{
			EventType = this.GetType().Name;
		}

		public static string Serialize<T>(T obj)
		{
			string payload = JsonSerializer.Serialize<T>(obj);
			return payload;
		}

		public static T Deserialize<T>(string jsonString)
		{
			T obj = JsonSerializer.Deserialize<T>(jsonString);
			return obj;
		}


	}
}
