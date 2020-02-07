using Dawn;
using Helpers.Tracing;
using OpenTracing;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetLatestLogEntryStep : IStepBody
	{
		private readonly Repositories.ICineworldRepository _cineworldRepository;
		private readonly ITracer? _tracer;

		public GetLatestLogEntryStep(
			Repositories.ICineworldRepository cineworldRepository,
			ITracer? tracer)
		{
			_cineworldRepository = Guard.Argument(() => cineworldRepository).NotNull().Value;
			_tracer = tracer;
		}

		public Models.LogEntry? LogEntry { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.BuildDefaultSpan().StartActive();

			LogEntry = await _cineworldRepository.GetLatestLogEntryAsync();

			scope?.Span.Log(nameof(LogEntry), LogEntry);

			return ExecutionResult.Next();
		}
	}
}
