using Dawn;
using MovieTimes.MovieDetailsService.Models;
using MovieTimes.MovieDetailsService.Models.Generated.TheMovieDb.Details;
using MovieTimes.MovieDetailsService.Models.Generated.TheMovieDb.Search;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Clients.Concrete
{
	public class TheMovieDbClient : ClientBase, ITheMovieDbClient
	{
		private readonly string _apiKey;

		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
			IgnoreNullValues = false,
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true,
		};

		public TheMovieDbClient(
			IHttpClientFactory httpClientFactory,
			string apiKey)
			: base(httpClientFactory)
		{
			_apiKey = Guard.Argument(() => apiKey).NotNull().NotEmpty().NotWhiteSpace().Value;
		}

		public async Task<IMovieDetails> DetailsAsync(int id)
		{
			Guard.Argument(() => id).Positive();

			var relativeUri = new Uri($"/3/movie/{id:D}?api_key={_apiKey}", UriKind.Relative);

			var (statusCode, headers, content) = await base.SendAsync(HttpMethod.Get, relativeUri);

			switch (statusCode)
			{
				case HttpStatusCode.OK:
					return await JsonSerializer.DeserializeAsync<Details>(content);
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

		public async IAsyncEnumerable<ISearchResult> SearchAsync(string query, int year)
		{
			Guard.Argument(() => query).NotNull().NotEmpty().NotWhiteSpace();
			Guard.Argument(() => year).InRange(1900, DateTime.Today.Year);

			var relativeUri = new Uri($"/3/search/movie?api_key={_apiKey}&query={query}&year={year:D}", UriKind.Relative);

			var (statusCode, headers, content) = await base.SendAsync(HttpMethod.Get, relativeUri);

			switch (statusCode)
			{
				case HttpStatusCode.OK:
					var search = await JsonSerializer.DeserializeAsync<Search>(content, _jsonSerializerOptions);

					foreach (var result in search.results!)
					{
						yield return result;
					}

					yield break;

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
