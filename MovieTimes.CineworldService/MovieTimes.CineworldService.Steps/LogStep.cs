using Dawn;
using Helpers.Tracing;
using MovieTimes.CineworldService.Repositories;
using OpenTracing;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Steps
{
	public class LogStep : IStepBody
	{
		private readonly ICineworldRepository _cineworldRepository;
		private readonly ITracer _tracer;

		public LogStep(
			ICineworldRepository cineworldRepository,
			ITracer tracer)
		{
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
		}

		public DateTime? LastModified { get; set; }
		public IScope? Scope { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => LastModified)
				.NotNull()
				.NotDefault()
				.Require(dt => dt.Kind == DateTimeKind.Utc, dt => $"{nameof(LastModified)} must be {nameof(DateTimeKind.Utc)}")
				.Max(DateTime.UtcNow);

			using var scope = _tracer
				.BuildDefaultSpan()
				.AsChildOf(Scope?.Span)
				.StartActive(finishSpanOnDispose: true);

#pragma warning disable CS8629 // Nullable value type may be null.
			scope.Span.Log(nameof(LastModified), LastModified.Value);

			await _cineworldRepository.LogAsync(LastModified.Value);
#pragma warning restore CS8629 // Nullable value type may be null.

			return ExecutionResult.Next();
		}
	}
}
