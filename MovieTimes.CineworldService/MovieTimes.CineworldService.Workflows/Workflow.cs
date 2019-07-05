using OpenTracing;
using System;
using System.Data;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.CineworldService.Workflows
{
	public class Workflow : IWorkflow<Models.PersistenceData>
	{
		public string Id => nameof(Workflow);
		public int Version => 1;

		public void Build(IWorkflowBuilder<Models.PersistenceData> builder)
		{
			builder
				.StartWith<BeginJaegerTraceStep>()
					.Input(step => step.OperationName, _ => "Get Cineworld listings")
					.Output(data => data.Scope, step => step.Scope)

				// Check database connectivity
				.While(data => (data.ConnectionState & ConnectionState.Open) == 0)
					.Do(nested => nested
						.StartWith<Steps.TestDatabaseConnectivityStep>()
							.Input(step => step.Scope, step => step.Scope)
							.Output(data => data.ConnectionState, step => step.ConnectionState)

						.If(data => (data.ConnectionState & ConnectionState.Open) == 0)
							.Do(then => then
								.StartWith(context => ExecutionResult.Sleep(TimeSpan.FromSeconds(30), context.Workflow.Data))
							)
					)

				// Get the local last-modified date (from the log)
				.Then<Steps.GetLocalLastModifiedStep>()
					.Input(step => step.Scope, step => step.Scope)
					.Output(data => data.LocalLastModified, step => step.LastModified)

				// Get the remote last-modified date (from Cineworld)
				.Then<Steps.GetListingsLastModifiedStep>()
					.Input(step => step.Scope, step => step.Scope)
					.Output(data => data.RemoteLastModified, step => step.LastModified)

				// If it's older than last time
				.If(data => data.LocalLastModified != default && data.RemoteLastModified <= data.LocalLastModified)
					.Do(then => then
						.StartWith<StopJaegerTraceStep>()
							.Input(step => step.Scope, step => step.Scope)

						.EndWorkflow()
					)

				// Get the listings
				.Then<Steps.GetCinemasStep>()
					.Input(step => step.Scope, step => step.Scope)
					.Output(data => data.Cinemas, step => step.Cinemas)

				// Save the listings
				.Then<Steps.SaveCinemasStep>()
					.Input(step => step.Cinemas, data => data.Cinemas)
					.Input(step => step.Scope, step => step.Scope)

				// Log
				.Then<Steps.LogStep>()
					.Input(step => step.LastModified, data => data.RemoteLastModified)
					.Input(step => step.Scope, step => step.Scope)

				.Then<StopJaegerTraceStep>()
					.Input(step => step.Scope, step => step.Scope)

				.EndWorkflow();
		}
	}

	public class BeginJaegerTraceStep : IStepBody
	{
		private readonly ITracer _tracer;

		public BeginJaegerTraceStep(
			ITracer tracer)
		{
			_tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
		}

		public string? OperationName { get; set; }
		public IScope? Scope { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			if (string.IsNullOrWhiteSpace(OperationName)) throw new ArgumentNullException(nameof(OperationName));

			Scope = _tracer
				.BuildSpan(OperationName)
				.StartActive(finishSpanOnDispose: false);

			_tracer.ScopeManager.Activate(Scope.Span, finishSpanOnDispose: false);

			return Task.FromResult(ExecutionResult.Next());
		}
	}

	public class StopJaegerTraceStep : IStepBody
	{
		public IScope? Scope { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Scope?.Span?.Finish();
			Scope?.Dispose();

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
