using eventstore.Events;
using StackExchange.Redis;

namespace eventstore.Services.Caching
{
	public class RedisCacheVersionProvider : ICacheVersionProvider
	{
		private readonly IConnectionMultiplexer _redis;
		private readonly ILogger<RedisCacheVersionProvider> _logger;

		public RedisCacheVersionProvider(IConnectionMultiplexer redis, ILogger<RedisCacheVersionProvider> logger)
		{
			_redis = redis;
			_logger = logger;
		}

		private static string VersionKey(string scope) => $"cache:version:{scope}";

		public async Task<long> GetVersionAsync(string scope, CancellationToken cancellationToken = default)
		{
			try
			{
				var value = await _redis.GetDatabase().StringGetAsync(VersionKey(scope));
				return value.TryParse(out long version) ? version : 0;
			}
			catch (RedisException ex)
			{
				// Redis is unreachable. Return a value that never repeats so the cache key is
				// always new and every request falls through to the database: slow, but correct.
				_logger.LogWarning(ex, "Could not read cache version for scope {Scope}; bypassing the cache.", scope);
				return -DateTime.UtcNow.Ticks;
			}
		}

		public async Task NotifyEventsAppendedAsync(IReadOnlyList<Event> appendedEvents, CancellationToken cancellationToken = default)
		{
			foreach (var scope in CacheScope.All)
			{
				if (!appendedEvents.Any(appended => scope.SourceEventTypes.Contains(appended.EventType)))
				{
					continue;
				}

				try
				{
					await _redis.GetDatabase().StringIncrementAsync(VersionKey(scope.Name));
				}
				catch (RedisException ex)
				{
					_logger.LogWarning(ex, "Could not bump the cache version for scope {Scope}.", scope.Name);
				}
			}
		}
	}
}
