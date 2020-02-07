using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Steps.Tests
{
	public class GetQueriesStepTests
	{
		[Fact]
		public async Task GetQueriesStepTests_BehavesPredictably()
		{
			var settings = new Helpers.MySql.Models.DbSettings
			{
				Database = "cineworld",
				Password = "xiebeiyoothohYaidieroh8ahchohphi",
				Port = 3_306,
				Server = "localhost",
				UserId = "movietimes",
			};

			var options = Mock.Of<IOptions<Helpers.MySql.Models.DbSettings>>(o => o.Value == settings);

			var repository = new Repositories.Concrete.QueriesRepository(options);

			var service = new Services.Concrete.QueriesService(repository);

			var sut = new Steps.GetQueriesStep(service, default);

			// Act
			await sut.RunAsync(Mock.Of<WorkflowCore.Interface.IStepExecutionContext>());

			// Assert
			Assert.NotEmpty(sut.Queries);

			foreach(var (id, query) in sut.Queries)
			{
				Assert.InRange(id, 1, short.MaxValue);
				Assert.NotNull(query);
				Assert.NotEmpty(query.CinemaIds);
			}
		}
	}
}
