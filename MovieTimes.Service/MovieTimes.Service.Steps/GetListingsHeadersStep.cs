using Dawn;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetListingsHeadersStep : IStepBody
	{
		private readonly Helpers.Cineworld.ICineworldClient _cineworldClient;

		public GetListingsHeadersStep(
			Helpers.Cineworld.ICineworldClient cineworldClient)
		{
			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;
		}

		public DateTime? LastModified { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			LastModified = await _cineworldClient.GetPerformancesLastModifiedDateAsync();

			return ExecutionResult.Next();
		}
	}
}
