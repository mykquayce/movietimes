using Dawn;
using Helpers.Cineworld.Models;
using System;
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

		public cinemasType? Cinemas { get; set; }
		public string? RelativeUri { get; set; }

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => RelativeUri!).NotNull().NotEmpty().NotWhiteSpace();

			var relativeUri = new Uri(RelativeUri!, UriKind.Relative);

			Cinemas = await _apiClient.RunQueryAsync<cinemasType>(relativeUri);

			return ExecutionResult.Next();
		}
	}
}
