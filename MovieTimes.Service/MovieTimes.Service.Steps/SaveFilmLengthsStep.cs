using Dawn;
using Helpers.Cineworld.Models.Generated;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class SaveFilmLengthsStep : IStepBody
	{
		private readonly Repositories.ICineworldRepository _repository;

		public SaveFilmLengthsStep(Repositories.ICineworldRepository repository)
		{
			_repository = Guard.Argument(() => repository).NotNull().Value;
		}

		public ICollection<FilmType>? Films { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => Films!).NotNull().NotEmpty().DoesNotContainNull();

			await _repository.SaveLengthsAsync(Films!);

			return ExecutionResult.Next();
		}
	}
}
