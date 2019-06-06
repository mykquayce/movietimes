using System.Collections.Generic;

namespace MovieTimes.MovieDetailsService.Repositories
{
	public interface IRepository
	{
		IAsyncEnumerable<(string title, short year)> GetMoviesMissingDetailsAsync();
	}
}
