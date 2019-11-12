using Dawn;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class SaveQueryResultStep : IStepBody
	{
		private readonly Repositories.IQueriesRepository _queriesRepository;

		public SaveQueryResultStep(Repositories.IQueriesRepository queriesRepository)
		{
			_queriesRepository = Guard.Argument(() => queriesRepository).NotNull().Value;
		}

		public KeyValuePair<int, string>? KeyValuePair { get; set; }
		public string? Json { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => KeyValuePair).NotNull();
			Guard.Argument(() => KeyValuePair!.Value.Key).Positive();
			Guard.Argument(() => Json).Require(s => s != default);
			Guard.Argument(() => Json!).NotNull().NotEmpty().NotWhiteSpace();

			await _queriesRepository.SaveQueryResult(KeyValuePair!.Value.Key, Json!);

			return ExecutionResult.Next();
		}
	}
}
