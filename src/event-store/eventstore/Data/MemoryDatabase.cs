using eventstore.Events;

namespace eventstore.Data
{
	public class MemoryDatabase
	{
		public List<Event> Events = new();
	}
}
