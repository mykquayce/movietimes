using MovieTimes.CineworldService.Models.Generated;
using System;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Clients
{
	public interface ICineworldClient
	{
		Task<DateTime> GetListingsLastModifiedAsync();
		Task<cinemas> GetListingsAsync();
	}
}
