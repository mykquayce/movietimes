using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Repositories.Tests
{
	public sealed class QueriesRepositoryTests : IDisposable
	{
		private readonly Services.ISerializationService _serializationService = new Services.Concrete.JsonSerializationService();

		private readonly IQueriesRepository _repository;

		public QueriesRepositoryTests()
		{
			var settings = new Helpers.MySql.Models.DbSettings
			{
				Server = "localhost",
				Port = 3_306,
				UserId = "movietimes",
				Password = "xiebeiyoothohYaidieroh8ahchohphi",
				Database = "cineworld",
			};

			var options = Mock.Of<IOptions<Helpers.MySql.Models.DbSettings>>(o => o.Value == settings);

			_repository = new Concrete.QueriesRepository(options);
		}

		[Fact]
		public async Task GetQueriesAsync()
		{
			var count = 0;

			await foreach (var (id, json) in _repository.GetQueriesAsync())
			{
				count++;

				Assert.InRange(id, 1, int.MaxValue);
				Assert.NotNull(json);
				Assert.NotEmpty(json);
			}

			Assert.InRange(count, 1, int.MaxValue);
		}

		[Fact]
		public async Task SaveQueryResultsAsync()
		{
			// Arange
			static DateTime f(string s)
				=> DateTime.Parse(s, provider: CultureInfo.InvariantCulture, styles: DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal);

			var collection = new Models.QueryResults
			{
				{ 23, "Cineworld Ashton-under-Lyne", "Jojo Rabbit", 108, f("2020-01-09T14:45:00Z") },
				{ 23, "Cineworld Ashton-under-Lyne", "Cats", 109, f("2020-01-09T15:00:00Z") },
			};

			var json = await SerializeAsync(collection);
			var checksum = Helpers.Common.ExtensionMethods.GetDeterministicHashCode(json);

			var queryResults = new Models.QueryResults(collection)
			{
				Checksum = checksum,
				Json = json,
				QueryId = 1,
			};

			try
			{
				// Act
				await _repository.SaveQueryResultsAsync(queryResults);
			}
			catch (Exception ex)
			{
				// Assert
				Assert.True(false, ex.Message);
			}
		}

		[Theory]
		[InlineData(1)]
		public async Task GetLastTwoQueryResultsCollectionsAsync(short queryId)
		{
			// Act
			var resultsCollection = _repository.GetLastTwoQueryResultsCollectionsAsync(queryId);

			await foreach (var results in resultsCollection)
			{
				// Assert
				Assert.NotNull(results.QueryId);
				Assert.InRange(results.QueryId!.Value, 1, int.MaxValue);
				Assert.NotNull(results.Json);
				Assert.NotEqual(0, results.Checksum);
			}
		}

		private async Task<string> SerializeAsync<T>(T value)
		{
			var stream = await _serializationService.SerializeAsync(value);
			using var reader = new StreamReader(stream, Encoding.UTF8);
			return await reader.ReadToEndAsync();
		}

		#region IDisposable implementation
		public void Dispose() => _repository?.Dispose();
		#endregion IDisposable implementation
	}

	public static class ExtensionMethods
	{
		public static Models.QueryResults Add(this Models.QueryResults queryResults, short cinemaId, string cinemaName, string filmTitle, short filmLength, DateTime showDateTime)
		{
			var queryResult = new Models.QueryResult
			{
				CinemaId = cinemaId,
				CinemaName = cinemaName,
				FilmTitle = filmTitle,
				FilmLength = filmLength,
				ShowDateTime = showDateTime,
			};

			queryResults.Add(queryResult);

			return queryResults;
		}
	}
}
