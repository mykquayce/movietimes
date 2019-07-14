using Dawn;
using Helpers.Tracing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MovieTimes.CineworldService.Clients.Concrete
{
	public sealed class CineworldClient : Helpers.HttpClient.HttpClientBase, ICineworldClient
	{
		private readonly ITracer? _tracer;
		private readonly Uri _listingsUri;
		private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(Models.Generated.cinemas));

		public CineworldClient(
			ILogger<CineworldClient>? logger,
			ITracer? tracer,
			IHttpClientFactory httpClientFactory,
			IOptions<Configuration.Uris> urisOptions)
			: base(httpClientFactory, logger, tracer)
		{
			_tracer = tracer;

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

		public async Task<Models.Generated.cinemas> GetListingsAsync()
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true);

			var (_, stream, _) = await base.SendAsync(HttpMethod.Get, _listingsUri);

			var cinemas = (Models.Generated.cinemas)_serializer.Deserialize(stream);

			scope?.Span.Log(
				"count", cinemas.cinema.Count);

			return cinemas;
		}

		public async Task<DateTime> GetListingsLastModifiedAsync()
		{
			using var scope = _tracer?
				.BuildDefaultSpan()
				.StartActive(finishSpanOnDispose: true);

			var (_, _, headers) = await base.SendAsync(HttpMethod.Head, _listingsUri);

			Guard.Argument(() => headers)
				.NotNull()
				.NotEmpty()
				.Require(d => d.ContainsKey("Last-Modified"));

			var value = headers["Last-Modified"];

			Guard.Argument(() => value)
				.NotNull()
				.NotEmpty()
				.DoesNotContainNull()
				.Count(1);

			var lastModifiedString = headers["Last-Modified"].Single();

			Guard.Argument(() => lastModifiedString)
				.NotNull()
				.NotEmpty()
				.NotWhiteSpace()
				.Matches(@"^\w{3}, \d{1,2} \w{3} \d{4} \d{2}:\d{2}:\d{2} GMT$");

			var lastModified = DateTime.Parse(lastModifiedString, styles: DateTimeStyles.AdjustToUniversal).ToUniversalTime();

			scope?.Span.Log(nameof(lastModified), lastModified);

			return lastModified;
		}
	}
}
