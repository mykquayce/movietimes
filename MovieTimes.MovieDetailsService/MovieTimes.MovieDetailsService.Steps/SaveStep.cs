using Dawn;
using MovieTimes.MovieDetailsService.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.MovieDetailsService.Steps
{
	public class SaveStep : IStepBody
	{
		private readonly IRepository _repository;

		public SaveStep(
			IRepository repository)
		{
			_repository = Guard.Argument(() => repository).NotNull().Value;
		}

		public ICollection<Models.PersistenceData.Movie>? Movies { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => Movies)
				.NotNull()
				.DoesNotContainNull();

			if (Movies.Any())
			{
				await _repository.SaveAsync(Movies);
			}


			return ExecutionResult.Next();
		}
	}
}
