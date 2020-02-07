using Dawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class BuildMessageStep : IStepBody
	{
		public IList<Models.QueryResults>? QueryResultsCollection { get; set; }
		public string? Message { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			if ((QueryResultsCollection?.Count ?? 0) == 0)
			{
				return Task.FromResult(ExecutionResult.Next());
			}

			var latest = (from rr in QueryResultsCollection!
						  orderby rr descending
						  select rr
						 ).First();

			Message = latest.ToString();

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
