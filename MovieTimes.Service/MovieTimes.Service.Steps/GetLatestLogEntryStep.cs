using Dawn;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetLatestLogEntryStep : IStepBody
	{
		private readonly Repositories.ICineworldRepository _cineworldRepository;

		public GetLatestLogEntryStep(
			Repositories.ICineworldRepository cineworldRepository)
		{
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
		}

		public Models.LogEntry? LogEntry { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			LogEntry = await _cineworldRepository.GetLatestLogEntryAsync();

			return ExecutionResult.Next();
		}
	}
}
