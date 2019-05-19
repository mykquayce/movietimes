using Dawn;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Clients.Concrete
{
	public class TheMovieDbClient : ClientBase, ITheMovieDbClient
	{
		private readonly string _apiKey;

		public TheMovieDbClient(
			IHttpClientFactory httpClientFactory,
			IOptions<Configuration.TheMovieDbSettings> settingsOptions)
			: base(httpClientFactory)
		{
			_apiKey = settingsOptions.Value.ApiKey;
		}

		public Task<string> DetailsAsync(int id)
		{
			throw new NotImplementedException();
		}

		public async Task<string> SearchAsync(string query)
		{
			Guard.Argument(() => query).NotNull().NotEmpty().NotWhiteSpace();

			var relativeUri = new Uri($"/3/search/movie?api_key={_apiKey}&query={query}", UriKind.Relative);

			var (statusCode, headers, content) = await base.SendAsync(HttpMethod.Get, relativeUri);

			switch (statusCode)
			{
				case HttpStatusCode.OK:
					return content;
				default:
					throw new Exception("Unexpected response from The Movie DB API")
					{
						Data =
						{
							[nameof(relativeUri)] = relativeUri,
							[nameof(statusCode)] = statusCode,
							[nameof(headers)] = headers,
							[nameof(content)] = content,
						},
					};
			}
		}
	}
}
