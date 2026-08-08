using eventstore.Events;

namespace eventstore.Models
{
	public abstract class DomainRoot
	{
		public int Id { get; set; }
		public int Version { get; set; }
		protected readonly List<Event> _unsavedEvents = [];

		protected abstract void Apply(Event @event);

		protected void RaiseEvent(Event @event)
		{
			Apply(@event);
			Version++;
			_unsavedEvents.Add(@event);
		}

		public IReadOnlyList<Event> DequeueUnsavedEvents()
		{
			var events = _unsavedEvents.ToList();
			_unsavedEvents.Clear();
			return events;
		}

		public void LoadEvents(List<Event> events)
		{
			// from db get all events where rootId == guid 
			
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
