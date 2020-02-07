using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Services.Tests
{
	public class QueriesServiceTests
	{
		[Fact]
		public async Task QueriesServiceTests_GetQueriesAsync()
		{
			var queriesRepositoryMock = new Mock<Repositories.IQueriesRepository>();

			var queries = new Dictionary<short, string>(6)
			{
				[1] = @"{ ""cinemaIds"": [ 23 ], ""timesOfDay"": ""Evening"", ""daysOfWeek"": ""Friday"", ""weekCount"": 1 }",
				[2] = @"{ ""cinemaIds"": [ 57 ], ""timesOfDay"": ""Evening"", ""daysOfWeek"": ""Friday"", ""weekCount"": 1 }",
				[3] = @"{ ""cinemaIds"": [ 96 ], ""timesOfDay"": ""Evening"", ""daysOfWeek"": ""Friday"", ""weekCount"": 1 }",
				[4] = @"{ ""cinemaIds"": [ 23 ], ""titles"": [ ""unlimited"", ""preview"" ] }",
				[5] = @"{ ""cinemaIds"": [ 57 ], ""titles"": [ ""unlimited"", ""preview"" ] }",
				[6] = @"{ ""cinemaIds"": [ 96 ], ""titles"": [ ""unlimited"", ""preview"" ] }",
			};

			var asyncEnumerable = queries.Select(kvp => (kvp.Key, kvp.Value)).ToAsyncEnumerable<(short, string)>();

			queriesRepositoryMock
				.Setup(r => r.GetQueriesAsync())
				.Returns(asyncEnumerable);

			var sut = new Services.Concrete.QueriesService(queriesRepositoryMock.Object);

			var count = 0;

			await foreach (var (id, query) in sut.GetQueriesAsync())
			{
				count++;

				Assert.InRange(id, 1, short.MaxValue);
				Assert.NotNull(query);
				Assert.NotEmpty(query.CinemaIds);
			}

			Assert.Equal(6, count);
		}
	}
}
