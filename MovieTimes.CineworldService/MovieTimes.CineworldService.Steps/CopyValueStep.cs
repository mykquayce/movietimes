using Dawn;
using Helpers.Tracing;
using OpenTracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Steps
{
	public class CopyValueStep : IStepBody
	{
		private readonly ITracer _tracer;

		public CopyValueStep(
			ITracer tracer)
		{
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
		}

		public object? PlaceHolder { get; set; }
		public ISpan? Span { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer
				.BuildDefaultSpan()
				.AsChildOf(Span)
				.StartActive(finishSpanOnDispose: true);

			scope.Span.Log(nameof(PlaceHolder), PlaceHolder?.ToString() ?? string.Empty);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
