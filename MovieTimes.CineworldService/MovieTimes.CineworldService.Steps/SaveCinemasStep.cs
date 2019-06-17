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
		public ISpan? Span { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			if (Cinemas == default
				|| Cinemas.cinema.Count == 0)
			{
				return ExecutionResult.Next();
			}

			using var _ = _tracer
				.BuildDefaultSpan()
				.AsChildOf(Span)
				.StartActive(finishSpanOnDispose: true);

			await _cineworldRepository.SaveCinemasAsync(Cinemas);

			return ExecutionResult.Next();
		}
	}
}
