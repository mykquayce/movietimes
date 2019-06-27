using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Repositories
{
	public interface IRepository
	{
		Task<IEnumerable<(int edi, string title)>> GetMoviesMissingMappingAsync();
		Task SaveAsync(ICollection<Models.PersistenceData.Movie> movies);
	}
}
