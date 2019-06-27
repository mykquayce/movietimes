using Dawn;
using MovieTimes.MovieDetailsService.Services;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.MovieDetailsService.Steps
{
	public class GetRuntimeStep : IStepBody
	{
		private readonly ITheMovieDbService _theMovieDbService;

		public GetRuntimeStep(
			ITheMovieDbService theMovieDbService)
		{
			_theMovieDbService = Guard.Argument(() => theMovieDbService).NotNull().Value;
		}

		public Models.PersistenceData.Movie? Movie { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => Movie).NotNull();
#pragma warning disable CS8602, CS8604
			Guard.Argument(() => Movie.Title).NotNull().NotEmpty().NotWhiteSpace();

			(Movie.Id, Movie.Title, Movie.ImdbId, Movie.Runtime) = await _theMovieDbService.DetailsAsync(Movie.Title);
#pragma warning restore CS8602, CS8604

			return ExecutionResult.Next();
		}
	}
}
