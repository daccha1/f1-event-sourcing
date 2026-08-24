
using eventstore.Data;
using eventstore.HostedServices;
using eventstore.Models;
using eventstore.Repositories;
using eventstore.Services.RabbitMQ;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
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

			builder.Services.AddScoped<IRaceRepository, RaceRepository>();
			builder.Services.AddScoped<IDriverRepository, DriversRepository>();
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
