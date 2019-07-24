using Dawn;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetListingsHeadersStep : IStepBody
	{
		private readonly Clients.ICineworldClient _cineworldClient;

		public GetListingsHeadersStep(
			Clients.ICineworldClient cineworldClient)
		{
			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;
		}

		public Models.HttpHeaders? HttpHeaders { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			HttpHeaders = await _cineworldClient.GetHeadersAsync();

			return ExecutionResult.Next();
		}
	}
}
