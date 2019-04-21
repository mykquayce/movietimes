using System;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Clients
{
	public interface ICineworldClient : IClient
	{
		Task<DateTime> GetListingsLastModifiedAsync();
		Task<string> GetListingsAsync();
	}
}
