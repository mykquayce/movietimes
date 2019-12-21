using Dawn;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetLastTwoResultsStep : IStepBody
	{
		private readonly Repositories.IQueriesRepository _queriesRepository;

		public GetLastTwoResultsStep(
			Repositories.IQueriesRepository queriesRepository)
		{
			_queriesRepository = Guard.Argument(() => queriesRepository).NotNull().Value;
		}

		public KeyValuePair<short, string>? KeyValuePair { get; set; }
		public ICollection<string> Results { get; } = new List<string>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => KeyValuePair).NotNull();
			Guard.Argument(() => KeyValuePair!.Value.Key).Positive();
			Guard.Argument(() => KeyValuePair!.Value.Value).NotNull().NotEmpty().NotWhiteSpace();

			await foreach (var result in _queriesRepository.GetLastTwoResultsAsync(KeyValuePair!.Value.Key))
			{
				Results.Add(result);
			}

			return ExecutionResult.Next();
		}
	}
}
