using Dawn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieTimes.CineworldService.Clients.Concrete
{
	public sealed class CineworldClient : Client, ICineworldClient
	{
		private readonly Uri _listingsUri;

		public CineworldClient(
			ILogger<CineworldClient>? logger,
			ITracer? tracer,
			IHttpClientFactory httpClientFactory,
			IOptions<Configuration.Uris> urisOptions)
			: base(logger, tracer, httpClientFactory)
		{
			Guard.Argument(() => urisOptions).NotNull();
			Guard.Argument(() => urisOptions.Value).NotNull();

			var listingsUriString = Guard.Argument(() => urisOptions.Value.ListingsUri)
				.NotNull()
				.NotEmpty()
				.NotWhiteSpace()
				.StartsWith("/")
				.Value;

			_listingsUri = new Uri(listingsUriString, UriKind.Relative);
		}

		public async Task<string> GetListingsAsync()
		{
			var (_, body, _) = await base.SendAsync(HttpMethod.Get, _listingsUri);

			return body;
		}

		public async Task<DateTime> GetListingsLastModifiedAsync()
		{
			var (_, _, headers) = await base.SendAsync(HttpMethod.Head, _listingsUri);

			return headers?.LastModified?.UtcDateTime ?? default;
		}
	}
}
