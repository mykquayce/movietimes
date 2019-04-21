using MovieTimes.CineworldService.Models.Generated;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Repositories
{
	public interface ICineworldRepository
	{
		Task SaveCinemasAsync(cinemas cinemas);
	}
}
