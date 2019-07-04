using Dawn;
using Helpers.Tracing;
using MovieTimes.CineworldService.Repositories;
using OpenTracing;
using System.Data;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Steps
{
	public class TestDatabaseConnectivityStep : IStepBody
	{
		private readonly ITracer _tracer;
		private readonly ICineworldRepository _cineworldRepository;

		public TestDatabaseConnectivityStep(
			ITracer tracer,
			ICineworldRepository cineworldRepository)
		{
			_tracer = Guard.Argument(() => tracer).NotNull().Value;
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
		}

		public ConnectionState? ConnectionState { get; private set; }
		public IScope? Scope { get; set; }

		public Task<ExecutionResult> RunAsync(
			IStepExecutionContext context)
		{
			using var scope = _tracer
				.BuildDefaultSpan()
				.AsChildOf(Scope?.Span)
				.StartActive(finishSpanOnDispose: true);

			_cineworldRepository.Connect();
			ConnectionState = _cineworldRepository.ConnectionState;

			scope.Span.Log(nameof(ConnectionState), ConnectionState);

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
