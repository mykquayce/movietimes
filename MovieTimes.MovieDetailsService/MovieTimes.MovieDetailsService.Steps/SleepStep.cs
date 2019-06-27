using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.MovieDetailsService.Steps
{
	public class SleepStep : IStepBody
	{
		public int MillisecondsDelay { get; set; } = 5_000;

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			var duration = TimeSpan.FromMilliseconds(MillisecondsDelay);
			var persistenceData = context?.Workflow?.Data ?? new object();
			var executionResult = ExecutionResult.Sleep(duration, persistenceData);

			return Task.FromResult(executionResult);
		}
	}
}
