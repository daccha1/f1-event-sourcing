using eventstore.Data;
using eventstore.Models;
using Microsoft.AspNetCore.Mvc;

namespace eventstore.Controllers
{

	public class ControllerHelper
	{
		public record Driver_Overtook(Guid driverFront, Guid driverBehind);
		public record Driver_FinishedRace(Guid driverId, Guid raceId, int position);
		public record Driver_StartRace(Guid driverId, Guid raceId);
		public record Driver_Disqualify(Guid driverId, string reason);
		public record Driver_Pitted(Guid driverId, char tyreType);
	}

	[ApiController]
	[Route("api/[controller]")]
	public class DriversController : ControllerBase
	{
		public IDriverRepository _repo;
		public DriversController(IDriverRepository repo)
		{
			_repo = repo;
		}
		
		[HttpPost]
		public IActionResult StartRace([FromBody] ControllerHelper.Driver_StartRace obj)
		{
			try
			{
				string str_driver = obj.driverId.ToString("N");
				string str_race = obj.raceId.ToString("N");
				Driver d = _repo.StartedTheRace(str_driver, str_race);
				return Ok(d);
			}
			catch (Exception ex)
			{
				return NotFound(ex.Message);
			}
		}

		[HttpGet("{driverId:guid}")]
		public IActionResult LoadDriver([FromRoute] Guid driverId)
		{
			string str_driver = driverId.ToString("N");
			var d = _repo.Load(str_driver);
			return Ok(d);
		}

		[HttpPost("finished")]
		public IActionResult FinishedRace([FromBody] ControllerHelper.Driver_FinishedRace obj)
		{
			string str_driver = obj.driverId.ToString("N");
			string str_race = obj.raceId.ToString("N");

			var d = _repo.FinishedRace(str_driver, obj.position);
			return Ok(d);
		}

		
		[HttpPost("overtake")]
		public IActionResult Overtook([FromBody] ControllerHelper.Driver_Overtook information)
		{
			string driver_front = information.driverFront.ToString("N");
			string driver_behind = information.driverBehind.ToString("N");

			Driver d = _repo.Overtook(driver_front, driver_behind);

			return Ok(d);
		}

		[HttpPost("disqualify")]
		public IActionResult Disqualified([FromBody] ControllerHelper.Driver_Disqualify information)
		{
			string driverId = information.driverId.ToString("N");
			Driver d = _repo.Disqualified(driverId, information.reason);
			return Ok(d);
		}

		[HttpPost("pit")]
		public IActionResult Pitted([FromBody] ControllerHelper.Driver_Pitted information)
		{
			string driverId = information.driverId.ToString("N");
			Driver d = _repo.Pitted(driverId, information.tyreType);
			return Ok(d);
		}

	}
}
