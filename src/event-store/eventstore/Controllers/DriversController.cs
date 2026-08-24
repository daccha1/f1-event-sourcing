using eventstore.Data;
using eventstore.Models;
using Microsoft.AspNetCore.Mvc;

namespace eventstore.Controllers
{

	public partial class ControllerHelper
	{
		public record Driver_Crashed(Guid driverId, DateTime occurredAt);
		public record Driver_Overtook(Guid driverFront, Guid driverBehind);
		public record Driver_FinishedRace(Guid driverId, Guid raceId, int position);
		public record Driver_StartRace(Guid driverId, Guid raceId, string name, string team);
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
		
		/// <summary>
		/// Records that a driver started a race.
		/// </summary>
		/// <param name="obj">The driver and race participation details.</param>
		[HttpPost]
		public IActionResult StartRace([FromBody] ControllerHelper.Driver_StartRace obj)
		{
			try
			{
				string str_driver = obj.driverId.ToString("N");
				string str_race = obj.raceId.ToString("N");
				//Driver d = _repo.StartedTheRace(str_driver, str_race);
				return Ok();
			}
			catch (Exception ex)
			{
				return NotFound(ex.Message);
			}
		}

		/// <summary>
		/// Rebuilds a driver's current state from its event stream.
		/// </summary>
		/// <param name="driverId">The race-specific driver identifier.</param>
		[HttpGet("{driverId:guid}")]
		public IActionResult LoadDriver([FromRoute] Guid driverId)
		{
			string str_driver = driverId.ToString();
			var d = _repo.Load(str_driver);
			return Ok(d);
		}

		/// <summary>
		/// Records a driver's finishing position.
		/// </summary>
		/// <param name="obj">The driver, race, and final position.</param>
		[HttpPost("finished")]
		public IActionResult FinishedRace([FromBody] ControllerHelper.Driver_FinishedRace obj)
		{
			string str_driver = obj.driverId.ToString("N");
			string str_race = obj.raceId.ToString("N");

			var d = _repo.FinishedRace(str_driver, obj.position);
			return Ok(d);
		}

		
		/// <summary>
		/// Records an overtake between two drivers.
		/// </summary>
		/// <param name="information">The driver ahead and the driver attempting the overtake.</param>
		[HttpPost("overtake")]
		public async Task<IActionResult> Overtook([FromBody] ControllerHelper.Driver_Overtook information)
		{
			string driver_front = information.driverFront.ToString("N");
			string driver_behind = information.driverBehind.ToString("N");

			Driver d = await _repo.Overtook(driver_front, driver_behind);

			return Ok(d);
		}

		/// <summary>
		/// Disqualifies a driver from a race.
		/// </summary>
		/// <param name="information">The driver and disqualification reason.</param>
		[HttpPost("disqualify")]
		public async Task<IActionResult> Disqualified([FromBody] ControllerHelper.Driver_Disqualify information)
		{
			string driverId = information.driverId.ToString("N");
			Driver d = await _repo.Disqualified(driverId, information.reason);
			return Ok(d);
		}

		/// <summary>
		/// Records a driver's pit stop and tyre choice.
		/// </summary>
		/// <param name="information">The driver and tyre type.</param>
		[HttpPost("pit")]
		public async Task<IActionResult> Pitted([FromBody] ControllerHelper.Driver_Pitted information)
		{
			string driverId = information.driverId.ToString("N");
			Driver d = await _repo.Pitted(driverId, information.tyreType);
			return Ok(d);
		}

	}
}
