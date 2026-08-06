
using eventstore.Data;
using eventstore.Models;
using eventstore.Repositories;

namespace eventstore
{
	public class Program
	{
		public static Race callCreation()
		{
			ForceRepo fr = new();
			return fr.CallCreation();
			
		}
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			
			// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
			builder.Services.AddOpenApi();


			Race r = callCreation();

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
