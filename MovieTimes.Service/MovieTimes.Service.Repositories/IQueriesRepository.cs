using System.Collections.Generic;

namespace MovieTimes.Service.Repositories
{
	public interface IQueriesRepository
	{
		IAsyncEnumerable<(int id, string query)> GetQueriesAsync();
	}
}
