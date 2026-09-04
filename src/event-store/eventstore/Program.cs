
using eventstore.Data;
using eventstore.HostedServices;
using eventstore.Models;
using eventstore.Repositories;
using eventstore.Services.Caching;
using eventstore.Services.RabbitMQ;
using eventstore.Services.Statistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.OpenApi;
using StackExchange.Redis;
using System.Diagnostics;

namespace eventstore
{
	public class Program
	{

		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			builder.Services.AddSqlServer<EventStoreDbContext>(
				builder.Configuration.GetConnectionString("EventStoreDatabase"));
			
			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();
			builder.Services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "F1 Event Store API",
					Version = "v1",
					Description = "API for recording Formula 1 race events and viewing race and championship statistics."
				});

				var xmlFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
			});

			var redisConnectionString =
				builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";

			// AbortOnConnectFail keeps the API bootable when Redis is not up yet: reads fall
			// back to the database instead of the whole service failing to start.
			ConfigurationOptions BuildRedisOptions() => new()
			{
				EndPoints = { redisConnectionString },
				AbortOnConnectFail = false,
				ConnectRetry = 3,
				// Short timeouts matter more than retries here: if Redis is unreachable the
				// request should fall through to the database in about a second, not hang on
				// the default ~5s connect plus ~5s command timeout.
				ConnectTimeout = 1000,
				SyncTimeout = 1000,
				AsyncTimeout = 1000
			};

			builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
				ConnectionMultiplexer.Connect(BuildRedisOptions()));

			builder.Services.AddStackExchangeRedisCache(options =>
			{
				options.ConfigurationOptions = BuildRedisOptions();
				options.InstanceName = "f1:";
			});

			// HybridCache puts an in-process layer in front of Redis and collapses concurrent
			// misses into a single rebuild, so a burst of requests cannot stampede the replay.
			builder.Services.AddHybridCache(options =>
			{
				options.DefaultEntryOptions = new HybridCacheEntryOptions
				{
					Expiration = TimeSpan.FromMinutes(10),
					LocalCacheExpiration = TimeSpan.FromMinutes(2)
				};
			});

			builder.Services.AddScoped<IRaceRepository, RaceRepository>();
			builder.Services.AddScoped<IDriverRepository, DriversRepository>();
			builder.Services.AddScoped<IStandingsProjection, StandingsProjection>();
			builder.Services.AddSingleton<ICacheVersionProvider, RedisCacheVersionProvider>();
			builder.Services.AddSingleton<MemoryDatabase>();
			builder.Services.AddSingleton<IMQClient, MQClient>();
			builder.Services.AddScoped<IReceivedMessageHandler, ReceivedMessageHandler>();

			builder.Services.AddHostedService<MQBackgroundService>();

			var app = builder.Build();
			using (var scope = app.Services.CreateScope())
			{
				var dbContext = scope.ServiceProvider.GetRequiredService<EventStoreDbContext>();
				dbContext.Database.Migrate();
			}

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
				app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					options.SwaggerEndpoint("/swagger/v1/swagger.json", "F1 Event Store API v1");
				});
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
