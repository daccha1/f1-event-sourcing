using eventstore.Models;

namespace eventstore.Data
{
	public interface IMemoryStore
	{
		public Race AddNew(string id, string ctr, string gp, int laps);
	}

}
