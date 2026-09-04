using eventstore.Events;

namespace eventstore.Services.Caching
{
	/// <summary>
	/// Keeps a monotonic generation number per cache scope. Cache keys embed the current
	/// generation, so appending a relevant event retires every entry in that scope at once
	/// instead of deleting keys one by one.
	/// </summary>
	public interface ICacheVersionProvider
	{
		Task<long> GetVersionAsync(string scope, CancellationToken cancellationToken = default);

		Task NotifyEventsAppendedAsync(IReadOnlyList<Event> appendedEvents, CancellationToken cancellationToken = default);
	}
}
