using Dawn;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class SaveLogEntryStep : IStepBody
	{
		private readonly Repositories.ICineworldRepository _cineworldRepository;

		public SaveLogEntryStep(
			Repositories.ICineworldRepository cineworldRepository)
		{
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
		}

		public DateTime? LastModified { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => LastModified)
				.NotNull();

			var logEntry = new Models.LogEntry { LastModified = LastModified, };

			await _cineworldRepository.LogAsync(logEntry);

			return ExecutionResult.Next();
		}
	}
}
