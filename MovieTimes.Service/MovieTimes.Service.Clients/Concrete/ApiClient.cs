using Microsoft.Extensions.Logging;
using OpenTracing;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieTimes.Service.Clients.Concrete
{
	public class ApiClient : Helpers.HttpClient.HttpClientBase, IApiClient
	{
		public ApiClient(IHttpClientFactory httpClientFactory, ILogger? logger = default, ITracer? tracer = default)
			: base(httpClientFactory, logger, tracer)
		{ }

		public async Task<T> RunQueryAsync<T>(Uri relativeUri)
		{
			var (_, value, _) = await base.SendAsync<T>(HttpMethod.Get, relativeUri);

			return value;
		}
	}
}
