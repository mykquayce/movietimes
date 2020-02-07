using Dawn;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class QueryResultsCollectionCheckerStep : IStepBody
	{
		public IList<Models.QueryResults>? QueryResultsCollection { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => QueryResultsCollection!).NotNull().NotEmpty().DoesNotContainNull();

			var outcome = (QueryResultsCollection?.Count ?? 0) switch
			{
				1 => true,
				2 => QueryResultsCollection![0].Checksum != QueryResultsCollection![1].Checksum,
				_ => throw new ArgumentOutOfRangeException(nameof(QueryResultsCollection)),
			};

			return Task.FromResult(ExecutionResult.Outcome(outcome));
		}
	}
}
