using Dawn;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetListingsStep : IStepBody
	{
		private readonly Clients.ICineworldClient _cineworldClient;

		public GetListingsStep(
			Clients.ICineworldClient cineworldClient)
		{
			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;
		}

		public Models.Generated.cinemas? Cinemas { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Cinemas = await _cineworldClient.GetListingsAsync();

			return ExecutionResult.Next();
		}
	}
}
