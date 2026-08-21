
using eventstore.Data;
using eventstore.HostedServices;
using eventstore.Models;
using eventstore.Repositories;
using eventstore.Services.RabbitMQ;
using Microsoft.EntityFrameworkCore;
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

			builder.Services.AddScoped<IRaceRepository, RaceRepository>();
			builder.Services.AddScoped<IDriverRepository, DriversRepository>();
			builder.Services.AddSingleton<MemoryDatabase>();
			builder.Services.AddSingleton<IMQClient, MQClient>();
			builder.Services.AddScoped<IReceivedMessageHandler, ReceivedMessageHandler>();

			builder.Services.AddHostedService<MQBackgroundService>();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.MapOpenApi();
			}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();

			app.Run();
		}
	}
}
