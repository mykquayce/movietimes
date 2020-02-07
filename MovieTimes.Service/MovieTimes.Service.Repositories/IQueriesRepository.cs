using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.Service.Repositories
{
	public interface IQueriesRepository : IDisposable
	{
		IAsyncEnumerable<Models.QueryResults> GetLastTwoQueryResultsCollectionsAsync(short queryId);
		IAsyncEnumerable<(short id, string json)> GetQueriesAsync();
		Task SaveQueryResultsAsync(Models.QueryResults queryResults);
	}
}
