
using eventstore.Data;
using eventstore.Models;
using eventstore.Repositories;
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
			
			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();

			builder.Services.AddSingleton<IMemoryStore, RaceMemoryStore>();
			builder.Services.AddSingleton<IDriverRepository, DriversRepository>();
			builder.Services.AddSingleton<MemoryDatabase>();

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
