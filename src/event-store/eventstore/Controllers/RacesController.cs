using eventstore.Data;
using Microsoft.AspNetCore.Mvc;

namespace eventstore.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class RacesController : ControllerBase
	{
		private IMemoryStore races;
		public RacesController(IMemoryStore races)
		{
			this.races = races;
		}

		[HttpPost]
		public IActionResult CreateRace([FromBody] Guid raceId)
		{
			string raceIdentificator = raceId.ToString("N");
			var r = races.AddNew(raceIdentificator, "Monaco", "MGP", 61);

			return Ok(r);
		}

		[HttpPost("finish")]
		public IActionResult FinishRace([FromBody] Guid raceId)
		{
			string raceIdentificator = raceId.ToString("N");

			var r = races.FinishRace(raceIdentificator);

			return Ok(r);
			
		}

		[HttpPost("stop")]
		public IActionResult StopRace([FromBody] Guid raceId, int lap)
		{
			string raceIdentificator = raceId.ToString("N");
			var r = races.StopRace(raceIdentificator, lap);
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
