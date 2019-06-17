using Dawn;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace MovieTimes.CineworldService.ConsoleApp
{
	public class HostedService : IHostedService
	{
		private readonly IWorkflowHost _workflowHost;

		public HostedService(
			IWorkflowHost workflowHost)
		{
			_workflowHost = Guard.Argument(() => workflowHost).NotNull().Value;

			_workflowHost.RegisterWorkflow<Workflows.Workflow, Models.PersistenceData>();

			_workflowHost.Start();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var persistenceData = new Models.PersistenceData();

			_workflowHost.StartWorkflow<Models.PersistenceData>(workflowId: nameof(Workflows.Workflow), data: persistenceData);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_workflowHost.Stop();
			return Task.CompletedTask;
		}
	}
}
