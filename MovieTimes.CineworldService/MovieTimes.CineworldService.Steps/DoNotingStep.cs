using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Steps
{
	public class DoNotingStep : IStepBody
	{
		public Task<ExecutionResult> RunAsync(IStepExecutionContext context) =>
			Task.FromResult(ExecutionResult.Next());
	}
}
