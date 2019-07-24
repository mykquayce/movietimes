using System;
using System.Threading.Tasks;

namespace MovieTimes.Service.Clients
{
	public interface ICineworldClient : IDisposable
	{
		Task<Models.HttpHeaders> GetHeadersAsync();
		Task<Models.Generated.cinemas> GetListingsAsync();
	}
}
