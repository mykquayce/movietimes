using Dawn;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetListingsStep : IStepBody
	{
		private readonly Helpers.Cineworld.ICineworldClient _cineworldClient;

		public GetListingsStep(
			Helpers.Cineworld.ICineworldClient cineworldClient)
		{
			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;
		}

		public Helpers.Cineworld.Models.cinemasType? Cinemas { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Cinemas = await _cineworldClient.GetPerformancesAsync();

			return ExecutionResult.Next();
		}
	}
}
