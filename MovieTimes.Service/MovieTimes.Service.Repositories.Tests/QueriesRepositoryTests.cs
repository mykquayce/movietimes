using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Repositories.Tests
{
	public class QueriesRepositoryTests
	{
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
		public async Task QueriesRepositoryTests_GetQueriesAsync()
		{
			var queries = new Dictionary<int, string>();

			await foreach (var (id, query) in _repository.GetQueriesAsync())
			{
				queries.Add(id, query);
			}

			Assert.Equal(2, queries.Count);
			Assert.Contains(1, queries.Keys);
			Assert.Contains(2, queries.Keys);
			Assert.NotNull(queries[1]);
			Assert.NotEmpty(queries[1]);
			Assert.NotNull(queries[2]);
			Assert.NotEmpty(queries[2]);
		}

		[Theory]
		[InlineData(1, "{}")]
		public async Task QueriesRepositoryTests_SaveQueryResult(int queryId, string json)
		{
			try
			{
				// Act
				await _repository.SaveQueryResult(queryId, json);
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch
#pragma warning restore CA1031 // Do not catch general exception types
			{
				// Assert
				Assert.True(false);
			}
		}
	}
}
