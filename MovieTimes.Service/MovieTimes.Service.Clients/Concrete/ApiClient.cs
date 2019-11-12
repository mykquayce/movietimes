using Microsoft.Extensions.Logging;
using OpenTracing;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieTimes.Service.Clients.Concrete
{
	public class ApiClient : Helpers.HttpClient.HttpClientBase, IApiClient
	{
		public ApiClient(IHttpClientFactory httpClientFactory, ILogger? logger = default, ITracer? tracer = default)
			: base(httpClientFactory, logger, tracer)
		{ }

		public async Task<string> RunQueryAsync(Uri relativeUri)
		{
			var (_, stream, _) = await base.SendAsync(HttpMethod.Get, relativeUri);

			using var reader = new StreamReader(stream);

			return await reader.ReadToEndAsync();
		}
	}
}
