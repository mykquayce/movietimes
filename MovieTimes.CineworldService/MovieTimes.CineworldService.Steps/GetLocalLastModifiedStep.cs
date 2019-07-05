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
	public class GetLocalLastModifiedStep : IStepBody
	{
		private readonly ICineworldRepository _cineworldRepository;
		private readonly ITracer _tracer;

		public GetLocalLastModifiedStep(
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
			using var scope = _tracer
				.BuildDefaultSpan()
				.AsChildOf(Scope?.Span)
				.StartActive(finishSpanOnDispose: true);

			LastModified = await _cineworldRepository.GetLastModifiedFromLogAsync();

			scope.Span.Log(nameof(LastModified), LastModified);

			return ExecutionResult.Next();
		}
	}
}
