using MovieTimes.MovieDetailsService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Clients
{
	public interface ITheMovieDbClient
	{
		Task<IMovieDetails> DetailsAsync(int id);
		IAsyncEnumerable<ISearchResult> SearchAsync(string query, int year);
	}
}
