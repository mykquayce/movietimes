using Helpers.Cineworld.Models;
using System.Collections.Generic;

namespace MovieTimes.Service.Services
{
	public interface IQueriesService
	{
		IAsyncEnumerable<(short id, Query)> GetQueriesAsync();
	}
}
