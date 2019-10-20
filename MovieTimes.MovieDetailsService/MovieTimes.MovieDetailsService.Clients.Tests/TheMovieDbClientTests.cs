using Moq;
using MovieTimes.MovieDetailsService.Clients.Concrete;
using MovieTimes.MovieDetailsService.Models.Generated.TheMovieDb.Search;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.MovieDetailsService.Clients.Tests
{
	public class TheMovieDbClientTests : IDisposable
	{
		private readonly ITheMovieDbClient _client;
		private readonly HttpClient _httpClient;

		private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
		{
			AllowTrailingCommas = true,
			DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
			IgnoreNullValues = false,
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = true,
		};

		public TheMovieDbClientTests()
		{
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri("https://api.themoviedb.org/", UriKind.Absolute),
			};

			var httpClientFactoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);

			httpClientFactoryMock
				.Setup(f => f.CreateClient(It.Is<string>(s => s == nameof(TheMovieDbClient))))
				.Returns(_httpClient);

			_client = new TheMovieDbClient(httpClientFactoryMock.Object, "40d01b496d7c0a0f2e9337669ac3fbbd");
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(obj: this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if(disposing)
			{
				_httpClient?.Dispose();
			}
		}

		[Theory]
		[InlineData("lego", 2019)]
		public async Task TheMovieDbClientTests_SearchAsync(string query, int year)
		{
			await foreach(var result in _client.SearchAsync(query, year))
			{
				Assert.NotNull(result);
				Assert.InRange(result.Id, 1, int.MaxValue);
				Assert.InRange(result.Popularity, 0d, double.MaxValue);
				Assert.NotNull(result.Title);
				Assert.InRange(result.ReleaseDate, new DateTime(1900, 1, 1), new DateTime(year + 1, 1, 1));
			}
		}

		[Theory]
		[InlineData(280217)]
		public async Task TheMovieDbClientTests_DetailsAsync(int id)
		{
			var detail = await _client.DetailsAsync(id);

			Assert.NotNull(detail.Title);
			Assert.InRange(detail.Popularity, 0d, double.MaxValue);
			Assert.NotNull(detail.Runtime);
			Assert.InRange(detail.Runtime?.TotalMinutes ?? 0d, 1, 100_000);
		}

		[Fact]
		public async Task Test()
		{
			var json = @"{""page"":1,""total_results"":1,""total_pages"":1,""results"":[{""vote_count"":55,""id"":533642,""video"":false,""vote_average"":5.7,""title"":""Child's Play"",""popularity"":84.239,""poster_path"":""\/xECBpdm4OXXlLajAlcvopXZFiQD.jpg"",""original_language"":""en"",""original_title"":""Child's Play"",""genre_ids"":[27],""backdrop_path"":""\/8Vjfen3Jit0nsMtu8huT09UqmsS.jpg"",""adult"":false,""overview"":""The story follows a mother named Karen, who gives her son Andy a toy doll for his birthday, unaware of its sinister nature."",""release_date"":""2019-06-19""}]}";

			var bytes = System.Text.Encoding.UTF8.GetBytes(json);

			Search search;

			using (var stream = new MemoryStream(bytes))
			{
				search = await JsonSerializer.DeserializeAsync<Search>(stream, _jsonSerializerOptions);
			}

			Assert.NotNull(search);
			Assert.NotEmpty(search.results);
			Assert.Single(search.results);
			Assert.All(search.results, Assert.NotNull);
		}
	}
}
