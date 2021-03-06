using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MovieTimes.Service.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace MovieTimes.Service.WorkerService
{
	public class Worker : BackgroundService
	{
		private const int _delay = 60 * 60 * 1_000;

		private readonly ILogger<Worker> _logger;
		private readonly IWorkflowHost _workflowHost;

		public Worker(
			ILogger<Worker> logger,
			IWorkflowHost workflowHost)
		{
			_logger = logger;

			_workflowHost = workflowHost;
			_workflowHost.OnStepError += WorkflowHost_OnStepError;
			_workflowHost.RegisterWorkflow<Workflows.Workflow, Models.PersistenceData>();
			_workflowHost.Start();
		}

		private void WorkflowHost_OnStepError(WorkflowCore.Models.WorkflowInstance workflow, WorkflowCore.Models.WorkflowStep step, Exception exception)
		{
			var message = exception.Message();
			var data = string.Join(';', from kvp in exception.Data()
										select $"{kvp.key}={kvp.value}");

			_logger?.LogError(exception, $"Workflow: {workflow?.Id}, step: {step?.Name}, threw {message}, {data}");
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var persistenceData = new Models.PersistenceData();

			while (!stoppingToken.IsCancellationRequested)
			{
				await _workflowHost.StartWorkflow(workflowId: nameof(Workflows.Workflow), data: persistenceData);

				await Task.Delay(_delay, stoppingToken);
			}

			await _workflowHost.StopAsync(stoppingToken);
		}
	}
}
