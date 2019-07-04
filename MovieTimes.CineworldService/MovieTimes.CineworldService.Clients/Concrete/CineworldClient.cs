using Dawn;
using Helpers.Tracing;
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
		private readonly ITracer _tracer;
		private readonly Uri _listingsUri;

		public CineworldClient(
			ILogger<CineworldClient> logger,
			ITracer tracer,
			IHttpClientFactory httpClientFactory,
			IOptions<Configuration.Uris> urisOptions)
			: base(logger, tracer, httpClientFactory)
		{
			_tracer = Guard.Argument(() => tracer).NotNull().Value;

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
			using var scope = _tracer
				.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true);

			var (_, body, _) = await base.SendAsync(HttpMethod.Get, _listingsUri);

			scope.Span.Log(
				"isnull", body == default,
				"length", body?.Length ?? default,
				"firstchar", body?[0] ?? default);

			return body ?? string.Empty;
		}

		public async Task<DateTime> GetListingsLastModifiedAsync()
		{
			using var scope = _tracer
				.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true);

			var (_, _, headers) = await base.SendAsync(HttpMethod.Head, _listingsUri);

			var lastModified = headers?.LastModified?.UtcDateTime ?? default;

			scope.Span.Log(nameof(lastModified), lastModified);

			return lastModified;
		}
	}
}
