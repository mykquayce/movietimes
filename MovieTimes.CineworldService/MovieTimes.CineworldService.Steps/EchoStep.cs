using Dawn;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Steps
{
	public class EchoStep : IStepBody
	{
		private readonly ILogger? _logger;

		public EchoStep(
			ILogger<EchoStep>? logger)
		{
			_logger = Guard.Argument(() => logger).NotNull().Value;
		}

		public string? Message { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			_logger?.LogInformation(Message);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
