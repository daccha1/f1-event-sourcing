using eventstore.Data;

namespace eventstore.Repositories
{
	public class ForceRepo
	{
		RaceMemoryStore rms = new();
		public Models.Race CallCreation()
		{
			Models.Race r = rms.AddNew("450B11AE-ABB2-442F-89A6-36F09BC86F30", "Monaco", "Monaco", 61);

			return r;

		}

	}
}
