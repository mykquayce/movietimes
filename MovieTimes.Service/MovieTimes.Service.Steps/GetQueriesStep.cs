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

		public IList<string> Queries { get; set; } = new List<string>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			await foreach (var (_, query) in _queriesRepository.GetQueriesAsync())
			{
				Queries.Add(query);
			}

			return ExecutionResult.Next();
		}
	}
}
