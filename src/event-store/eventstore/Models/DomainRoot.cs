using eventstore.Events;

namespace eventstore.Models
{
	public abstract class DomainRoot
	{
		public int Id { get; set; }
		public int Version { get; set; }

		protected abstract void Apply(Event @event);

		public void LoadEvents(List<Event> events)
		{
			foreach (var evt in events)
			{
				Apply(evt);
				Version++;
			}
		}

		// snapshot
		// restore snapshot
	}
}
