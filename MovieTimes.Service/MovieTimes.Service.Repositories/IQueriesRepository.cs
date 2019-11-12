using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories
{
	public interface IQueriesRepository
	{
		IAsyncEnumerable<(int id, string query)> GetQueriesAsync();
		Task SaveQueryResult(int id, string json);
	}
}
