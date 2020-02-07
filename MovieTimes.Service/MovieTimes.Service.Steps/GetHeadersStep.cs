using Dawn;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetHeadersStep : IStepBody
	{
		private readonly Helpers.Cineworld.ICineworldClient _cineworldClient;

		public GetHeadersStep(
			Helpers.Cineworld.ICineworldClient cineworldClient)
		{
			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;
		}

		public DateTime? LastModified { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			LastModified = await _cineworldClient.GetLastModifiedDateAsync();

			return ExecutionResult.Next();
		}
	}
}
