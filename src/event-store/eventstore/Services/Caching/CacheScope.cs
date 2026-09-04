namespace eventstore.Services.Caching
{
	/// <summary>
	/// A named group of cached entries together with the stored event types that make them stale.
	/// Adding a new cached read model means adding one entry here; nothing else needs to know
	/// which cache keys currently exist.
	/// </summary>
	public sealed record CacheScope(string Name, IReadOnlySet<string> SourceEventTypes)
	{
		/// <summary>
		/// Championship standings are rebuilt from driver participation and finishing positions
		/// only, so overtakes, pit stops and crashes do not invalidate them.
		/// </summary>
		public static readonly CacheScope Standings = new(
			"standings",
			new HashSet<string> { "StartedRace", "FinishedRace" });

		public static readonly IReadOnlyList<CacheScope> All = [Standings];
	}
}
