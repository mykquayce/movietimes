using Dawn;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTracing;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MovieTimes.Service.Clients.Concrete
{
	public class CineworldClient : Helpers.HttpClient.HttpClientBase, ICineworldClient
	{
		private readonly Uri _listingsUri;
		private readonly XmlSerializer _xmlSerializer;

		public CineworldClient(
			IOptions<Configuration.Uris> options,
			IHttpClientFactory httpClientFactory,
			ILogger<ICineworldClient>? logger = default,
			ITracer? tracer = default)
			: base(httpClientFactory, logger, tracer)
		{
			Guard.Argument(() => options).NotNull();
			Guard.Argument(() => options.Value).NotNull();
			Guard.Argument(() => options.Value.ListingsUri).NotNull().NotEmpty().NotWhiteSpace();

			_listingsUri = new Uri(options.Value.ListingsUri, UriKind.Relative);

			_xmlSerializer = new XmlSerializer(typeof(Models.Generated.cinemas));
		}

		public async Task<Models.HttpHeaders> GetHeadersAsync()
		{
			var (_, _, headers) = await base.SendAsync(HttpMethod.Head, _listingsUri);

			string? GetValue(string key) => headers.TryGetValue(key, out var values) ? values.SingleOrDefault() : default;

			long? GetContentLength() => long.TryParse(GetValue("Content-Length"), out var l) ? l : default(long?);

			DateTime? GetLastModified() => DateTime.TryParse(GetValue("Last-Modified"), provider: CultureInfo.InvariantCulture, styles: DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt)
				? dt
				: default(DateTime?);

			return new Models.HttpHeaders
			{
				ContentLength = GetContentLength(),
				LastModified = GetLastModified(),
			};
		}

		public async Task<Models.Generated.cinemas> GetListingsAsync()
		{
			var (_, stream, _) = await base.SendAsync(HttpMethod.Get, _listingsUri);

			using (stream)
			{
				return (Models.Generated.cinemas)_xmlSerializer.Deserialize(stream);
			}
		}
	}
}
