using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace MovieTimes.Service.ConsoleApp
{
	public class HostedService : IHostedService, IDisposable
	{
		private Timer? _timer;
		private readonly IWorkflowHost _workflowHost;

		public HostedService(IWorkflowHost workflowHost)
		{
			_workflowHost = workflowHost;

			_workflowHost.RegisterWorkflow<Workflows.Workflow, Models.PersistenceData>();

			_workflowHost.Start();
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_timer = new Timer(DoWork, state: default, dueTime: TimeSpan.Zero, period: TimeSpan.FromHours(1));
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_timer?.Change(dueTime: Timeout.Infinite, period: 0);
			_workflowHost.Stop();
			return Task.CompletedTask;
		}

		private void DoWork(object state)
		{
			var persistenceData = new Models.PersistenceData();
			_workflowHost.StartWorkflow<Models.PersistenceData>(workflowId: nameof(Workflows.Workflow), data: persistenceData);
		}
	}
}
