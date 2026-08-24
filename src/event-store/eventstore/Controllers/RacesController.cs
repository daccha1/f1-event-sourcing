using eventstore.Data;
using Microsoft.AspNetCore.Mvc;

namespace eventstore.Controllers
{

	public partial class ControllerHelper
	{
		public record CreateRace(Guid raceId, string country, string gp, int laps);
		public record FinishRace(Guid raceId);
		public record StopRace(Guid raceId, int lap);
	}

	[ApiController]
	[Route("api/[controller]")]
	public class RacesController : ControllerBase
	{
		private IRaceRepository races;
		public RacesController(IRaceRepository races)
		{
			this.races = races;
		}

		/// <summary>
		/// Creates a race event stream.
		/// </summary>
		/// <param name="createRace">The race country, Grand Prix name, lap count, and identifier.</param>
		[HttpPost]
		public async Task<ActionResult> CreateRace([FromBody] ControllerHelper.CreateRace createRace)
		{
			string raceIdentificator = createRace.raceId.ToString("N");
			var r = await races.AddNew(raceIdentificator, createRace.country, createRace.gp, createRace.laps);

			return Ok(r);
		}

		/// <summary>
		/// Marks a race as finished.
		/// </summary>
		/// <param name="finishRace">The race to finish.</param>
		[HttpPost("finish")]
		public async Task<ActionResult> FinishRace([FromBody] ControllerHelper.FinishRace finishRace)
		{
			string raceIdentificator = finishRace.raceId.ToString("N");

			var r = await races.FinishRace(raceIdentificator);

			return Ok(r);
			
		}

		/// <summary>
		/// Stops a race at the specified lap.
		/// </summary>
		/// <param name="stopRace">The race and lap where it was stopped.</param>
		[HttpPost("stop")]
		public async Task<ActionResult> StopRace([FromBody] ControllerHelper.StopRace stopRace)
		{
			string raceIdentificator = stopRace.raceId.ToString("N");
			var r = await races.StopRace(raceIdentificator, stopRace.lap);
			return Ok(r);
		}

		/// <summary>
		/// Rebuilds the current state of a race from its event stream.
		/// </summary>
		/// <param name="raceId">The race identifier.</param>
		[HttpGet("{raceId:guid}")]
		public async Task<ActionResult> LoadRace([FromRoute] Guid raceId)
		{
			string raceIdentificator = raceId.ToString("N");
			var r = await races.Load(raceIdentificator);

			return Ok(r);
		}

	}
}
