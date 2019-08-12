using Dawn;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class RunQueryStep : IStepBody
	{
		public string? RelativeUri { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => RelativeUri).NotNull().NotEmpty().NotWhiteSpace();

			var relativeUri = new Uri(RelativeUri!, UriKind.Relative);


			throw new NotImplementedException();
		}
	}
}
