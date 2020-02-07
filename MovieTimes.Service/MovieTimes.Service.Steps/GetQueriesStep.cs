using Dawn;
using Helpers.Tracing;
using OpenTracing;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetQueriesStep : IStepBody
	{
		private readonly Services.IQueriesService _queriesService;
		private readonly ITracer? _tracer;

		public GetQueriesStep(Services.IQueriesService queriesService, ITracer? tracer)
		{
			_queriesService = Guard.Argument(() => queriesService).NotNull().Value;
			_tracer = tracer;
		}

		public IDictionary<short, Helpers.Cineworld.Models.Query> Queries { get; } = new Dictionary<short, Helpers.Cineworld.Models.Query>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			using var scope = _tracer?.BuildDefaultSpan().StartActive();

			await foreach (var (id, query) in _queriesService.GetQueriesAsync())
			{
				Queries.Add(id, query);

				scope?.Span.Log(id, query);
			}

			return ExecutionResult.Next();
		}
	}
}
