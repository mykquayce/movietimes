using System;
using System.Threading.Tasks;

namespace MovieTimes.Service.Clients
{
	public interface IApiClient : IDisposable
	{
		Task<string> RunQueryAsync(Uri relativeUri);
	}
}
