using Dawn;
using Helpers.Tracing;
using MovieTimes.CineworldService.Clients;
using OpenTracing;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Steps
{
	public class GetListingsLastModifiedStep : IStepBody
	{
		private readonly ITracer _tracer;
		private readonly ICineworldClient _cineworldClient;

		public GetListingsLastModifiedStep(
			ITracer tracer,
			ICineworldClient cineworldClient)
		{
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
			_cineworldClient = Guard.Argument(() => cineworldClient).NotNull().Value;
		}

		public DateTime LastModified { get; private set; }
		public ISpan? Span { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer
				.BuildDefaultSpan()
				.AsChildOf(Span)
				.StartActive(finishSpanOnDispose: true);

			LastModified = await _cineworldClient.GetListingsLastModifiedAsync();

			scope.Span.Log(nameof(LastModified), LastModified);

			return ExecutionResult.Next();
		}
	}
}
