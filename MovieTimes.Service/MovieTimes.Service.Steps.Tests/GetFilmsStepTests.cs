using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Steps.Tests
{
	public class GetFilmsStepTests
	{
		[Fact]
		public async Task GetFilmLengthsStepTests_RunAsync()
		{
			var cineworldClient = new Helpers.Cineworld.Concrete.CineworldClient();

			var sut = new GetFilmsStep(cineworldClient);

			var stepExecutionContext = new WorkflowCore.Models.StepExecutionContext();

			await sut.RunAsync(stepExecutionContext);

			Assert.NotNull(sut.Films);
			Assert.NotEmpty(sut.Films);

			foreach (var film in sut.Films!)
			{
				Assert.NotNull(film);
				Assert.InRange(film.Edi, 0, int.MaxValue);
			}
		}
	}
}
