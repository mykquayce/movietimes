using Dawn;
using MovieTimes.MovieDetailsService.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.MovieDetailsService.Steps
{
	public class GetMoviesMissingMappingStep : IStepBody
	{
		private readonly IRepository _repository;

		public GetMoviesMissingMappingStep(
			IRepository repository)
		{
			_repository = Guard.Argument(() => repository).NotNull().Value;
		}

		public ICollection<Models.PersistenceData.Movie> Movies { get; } = new List<Models.PersistenceData.Movie>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			foreach (var (edi, title) in await _repository.GetMoviesMissingMappingAsync())
			{
				var movie = new Models.PersistenceData.Movie
				{
					Edi = edi,
					Title = title,
				};

				Movies.Add(movie);
			}

			return ExecutionResult.Next();
		}
	}
}
