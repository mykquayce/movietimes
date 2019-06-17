using Dawn;
using Helpers.Tracing;
using OpenTracing;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Steps
{
	public class SleepStep : IStepBody
	{
		private readonly ITracer _tracer;

		public SleepStep(
			ITracer tracer)
		{
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
		}

		public int MillisecondsDelay { get; set; } = 5_000;
		public ISpan? Span { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer
				.BuildDefaultSpan()
				.AsChildOf(Span)
				.StartActive(finishSpanOnDispose: true);

			scope.Span.Log(nameof(MillisecondsDelay), MillisecondsDelay);

			var duration = TimeSpan.FromMilliseconds(MillisecondsDelay);
			var persistenceData = context?.Workflow?.Data ?? new Object();
			var executionResult = ExecutionResult.Sleep(duration, persistenceData);

			return Task.FromResult(executionResult);
		}
	}
}
