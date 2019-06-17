using Helpers.Tracing;
using OpenTracing;
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
				.StartWith<Steps.DoNotingStep>()

				// Begin infinite loop
				.While(_ => true)
					.Do(workflow => workflow
						.StartWith<StartJaegerTrace>()
							.Output(data => data.Span, step => step.Span)

						// Check database connectivity
						.While(data => (data.ConnectionState & ConnectionState.Open) == 0)
							.Do(nested => nested
								.StartWith<Steps.TestDatabaseConnectivityStep>()
									.Input(step => step.Span, data => data.Span)
									.Output(data => data.ConnectionState, step => step.ConnectionState)
								.If(data => (data.ConnectionState & ConnectionState.Open) == 0)
									.Do(then => then
									.StartWith<Steps.SleepStep>()
										.Input(step => step.Span, data => data.Span)
										.Input(step => step.MillisecondsDelay, _ => 30_000)
									)
							)

						// Get last-modified date
						.Then<Steps.GetListingsLastModifiedStep>()
							.Input(step => step.Span, data => data.Span)
							.Output(data => data.CurrentLastModified, step => step.LastModified)

						// If it's newer than last time
						.If(data => data.PreviousLastModified == default || data.CurrentLastModified > data.PreviousLastModified)
							.Do(then => then

								// Get the listings
								.StartWith<Steps.GetCinemasStep>()
									.Input(step => step.Span, data => data.Span)
									.Output(data => data.Cinemas, step => step.Cinemas)

								// Save the listings
								.Then<Steps.SaveCinemasStep>()
									.Input(step => step.Span, data => data.Span)
									.Input(step => step.Cinemas, data => data.Cinemas)

								// Save the last-modified date
								.Then<Steps.CopyValueStep>()
									.Input(step => step.Span, data => data.Span)
									.Input(step => step.PlaceHolder, data => data.CurrentLastModified)
									.Output(data => data.PreviousLastModified, step => step.PlaceHolder)
							)

						.Then<StopJaegerTrace>()
							.Input(step => step.Span, data => data.Span)

						// Sleep
						.Then<Steps.SleepStep>()
							.Input(step => step.MillisecondsDelay, _ => 3_600_000)

					// Loop
					);
		}
	}

	public class StartJaegerTrace : IStepBody
	{
		private readonly ITracer _tracer;

		public StartJaegerTrace(
			ITracer tracer)
		{
			_tracer = tracer;
		}

		public ISpan? Span { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Span = _tracer.BuildDefaultSpan().Start();

			_tracer.ScopeManager.Activate(Span, finishSpanOnDispose: true);

			return Task.FromResult(ExecutionResult.Next());
		}
	}

	public class StopJaegerTrace : IStepBody
	{
		public ISpan? Span { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Span?.Finish();

			return Task.FromResult(ExecutionResult.Next());
		}
	}
}
