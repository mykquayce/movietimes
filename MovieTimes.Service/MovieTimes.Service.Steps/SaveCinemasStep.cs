using Dawn;
using Helpers.Cineworld.Models.Generated;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class SaveCinemasStep : IStepBody
	{
		private readonly Repositories.ICineworldRepository _cineworldRepository;

		public SaveCinemasStep(
			Repositories.ICineworldRepository cineworldRepository)
		{
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
		}

		public ICollection<CinemaType>? Cinemas { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => Cinemas!).NotNull();

			await _cineworldRepository.SaveCinemasAsync(Cinemas!);

			return ExecutionResult.Next();
		}
	}
}
