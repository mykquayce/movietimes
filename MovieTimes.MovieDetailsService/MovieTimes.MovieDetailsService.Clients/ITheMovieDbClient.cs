using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Clients
{
	public interface ITheMovieDbClient
	{
		Task<string> DetailsAsync(int id);
		Task<string> SearchAsync(string query);
	}
}
