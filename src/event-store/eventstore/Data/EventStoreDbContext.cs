using eventstore.Events;
using Microsoft.EntityFrameworkCore;

namespace eventstore.Data
{
	public class EventStoreDbContext : DbContext
	{
		public EventStoreDbContext(DbContextOptions<EventStoreDbContext> options)
			: base(options)
		{
		}

		public DbSet<Event> Events { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Event>(entity =>
			{
				// Aggregates are rebuilt by replaying one stream at a time, and projections
				// filter the log by event type. Both are read on every request, so neither
				// should fall back to scanning the whole log.
				entity.HasIndex(evt => evt.RootId);
				entity.HasIndex(evt => evt.EventType);
			});
		}
	}
}
