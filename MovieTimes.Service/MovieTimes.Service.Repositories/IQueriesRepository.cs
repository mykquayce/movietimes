using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories
{
	public interface IQueriesRepository
	{
		IAsyncEnumerable<string> GetLastTwoResultsAsync(short queryId);
		IAsyncEnumerable<(short id, string json)> GetQueriesAsync();
		Task SaveQueryResult(short id, string json);
	}
}
