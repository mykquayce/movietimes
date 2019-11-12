using Dawn;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetQueriesStep : IStepBody
	{
		private readonly Repositories.IQueriesRepository _queriesRepository;

		public GetQueriesStep(Repositories.IQueriesRepository queriesRepository)
		{
			_queriesRepository = Guard.Argument(() => queriesRepository).NotNull().Value;
		}

		public IDictionary<int, string> Queries { get; } = new Dictionary<int, string>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			await foreach (var (id, query) in _queriesRepository.GetQueriesAsync())
			{
				Queries.Add(id, query);
			}

			return ExecutionResult.Next();
		}
	}
}
