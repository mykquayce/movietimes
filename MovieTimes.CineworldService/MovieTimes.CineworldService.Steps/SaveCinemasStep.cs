using Dawn;
using Helpers.Tracing;
using MovieTimes.CineworldService.Repositories;
using OpenTracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Steps
{
	public class SaveCinemasStep : IStepBody
	{
		private readonly ITracer _tracer;
		private readonly ICineworldRepository _cineworldRepository;

		public SaveCinemasStep(
			ITracer tracer,
			ICineworldRepository cineworldRepository)
		{
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
		}

		public Models.Generated.cinemas? Cinemas { get; private set; }
		public IScope? Scope { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var _ = _tracer
				.BuildDefaultSpan()
				.AsChildOf(Scope?.Span)
				.StartActive(finishSpanOnDispose: true);

			if (Cinemas?.cinema.Count > 0)
			{
				await _cineworldRepository.SaveCinemasAsync(Cinemas);
			}

			return ExecutionResult.Next();
		}
	}
}
