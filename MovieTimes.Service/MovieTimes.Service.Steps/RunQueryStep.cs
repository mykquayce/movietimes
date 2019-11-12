using Dawn;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class RunQueryStep : IStepBody
	{
		private readonly Clients.IApiClient _apiClient;

		public RunQueryStep(Clients.IApiClient apiClient)
		{
			_apiClient = Guard.Argument(() => apiClient).NotNull().Value;
		}

		public string? Json { get; set; }
		public KeyValuePair<int, string>? KeyValuePair { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => KeyValuePair).NotNull();
			Guard.Argument(() => KeyValuePair!.Value.Value).NotNull().NotEmpty().NotWhiteSpace();

			var relativeUri = new Uri(KeyValuePair!.Value.Value, UriKind.Relative);

			Json = await _apiClient.RunQueryAsync(relativeUri);

			return ExecutionResult.Next();
		}
	}
}
