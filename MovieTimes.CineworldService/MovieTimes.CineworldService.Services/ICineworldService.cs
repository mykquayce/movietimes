using MovieTimes.CineworldService.Models.Generated;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Services
{
	public interface ICineworldService
	{
		Task<cinemas> GetCinemasAsync();
	}
}
