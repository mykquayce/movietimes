using MovieTimes.CineworldService.Models.Generated;
using System;
using System.Data;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Repositories
{
	public interface ICineworldRepository
	{
		ConnectionState ConnectionState { get; }
		void Connect();
		Task<DateTime> GetDateTimeAsync();
		Task SaveCinemasAsync(cinemas cinemas);
	}
}
