using System;
using System.Threading.Tasks;

namespace MovieTimes.Service.Clients
{
	public interface IApiClient
	{
		Task<T> RunQueryAsync<T>(Uri relativeUri);
	}
}
