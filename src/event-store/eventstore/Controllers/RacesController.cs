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

		[HttpPost]
		public IActionResult CreateRace([FromBody] ControllerHelper.CreateRace createRace)
		{
			string raceIdentificator = createRace.raceId.ToString("N");
			var r = races.AddNew(raceIdentificator, createRace.country, createRace.gp, createRace.laps);

			return Ok(r);
		}

		[HttpPost("finish")]
		public IActionResult FinishRace([FromBody] ControllerHelper.FinishRace finishRace)
		{
			string raceIdentificator = finishRace.raceId.ToString("N");

			var r = races.FinishRace(raceIdentificator);

			return Ok(r);
			
		}

		[HttpPost("stop")]
		public IActionResult StopRace([FromBody] ControllerHelper.StopRace stopRace)
		{
			string raceIdentificator = stopRace.raceId.ToString("N");
			var r = races.StopRace(raceIdentificator, stopRace.lap);
			return Ok(r);
		}

		[HttpGet("{raceId:guid}")]
		public IActionResult LoadRace([FromRoute] Guid raceId)
		{
			string raceIdentificator = raceId.ToString("N");
			var r = races.Load(raceIdentificator);

			return Ok(r);
		}

	}
}
