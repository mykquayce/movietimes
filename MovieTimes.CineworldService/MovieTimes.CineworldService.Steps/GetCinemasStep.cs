using Dawn;
using Helpers.Tracing;
using MovieTimes.CineworldService.Models.Helpers;
using MovieTimes.CineworldService.Services;
using OpenTracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Steps
{
	public class GetCinemasStep : IStepBody
	{
		private readonly ITracer _tracer;
		private readonly ICineworldService _cineworldService;

		public GetCinemasStep(
			ITracer tracer,
			ICineworldService cineworldService)
		{
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
			_cineworldService = Guard.Argument(() => cineworldService).NotNull().Value;
		}

		public Models.Generated.cinemas? Cinemas { get; private set; }
		public IScope? Scope { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer
				.BuildDefaultSpan()
				.AsChildOf(Scope?.Span)
				.StartActive(finishSpanOnDispose: true);

			Cinemas = await _cineworldService.GetCinemasAsync();

			var (cinemaCount, filmCount, showCount) = Cinemas.GetCounts();

			scope.Span.Log(
				nameof(cinemaCount), cinemaCount,
				nameof(filmCount), filmCount,
				nameof(showCount), showCount);

			return ExecutionResult.Next();
		}
	}
}
