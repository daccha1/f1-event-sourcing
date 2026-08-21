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
	}
}