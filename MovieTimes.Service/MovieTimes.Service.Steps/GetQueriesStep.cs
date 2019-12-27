using Dawn;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetQueriesStep : IStepBody
	{
		private readonly Services.IQueriesService _queriesService;

		public GetQueriesStep(Services.IQueriesService queriesService)
		{
			_queriesService = Guard.Argument(() => queriesService).NotNull().Value;
		}

		public IDictionary<short, Helpers.Cineworld.Models.Query> Queries { get; } = new Dictionary<short, Helpers.Cineworld.Models.Query>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			await foreach (var (id, query) in _queriesService.GetQueriesAsync())
			{
				Queries.Add(id, query);
			}

			return ExecutionResult.Next();
		}
	}
}
