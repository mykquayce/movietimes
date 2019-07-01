using System;
using System.Data;
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
				.StartWith(context => ExecutionResult.Next())

				// Check database connectivity
				.While(data => (data.ConnectionState & ConnectionState.Open) == 0)
					.Do(nested => nested
						.StartWith<Steps.TestDatabaseConnectivityStep>()
							.Output(data => data.ConnectionState, step => step.ConnectionState)

						.If(data => (data.ConnectionState & ConnectionState.Open) == 0)
							.Do(then => then
								.StartWith(context => ExecutionResult.Sleep(TimeSpan.FromSeconds(30), context.Workflow.Data))
							)
					)

				// Get last-modified date
				.Then<Steps.GetListingsLastModifiedStep>()
					.Output(data => data.CurrentLastModified, step => step.LastModified)

				// If it's older than last time
				.If(data => data.PreviousLastModified != default && data.CurrentLastModified < data.PreviousLastModified)
					.Do(then => then
						.StartWith(context => ExecutionResult.Next())
						.EndWorkflow()
					)

				// Get the listings
				.Then<Steps.GetCinemasStep>()
					.Output(data => data.Cinemas, step => step.Cinemas)

				// Save the listings
				.Then<Steps.SaveCinemasStep>()
					.Input(step => step.Cinemas, data => data.Cinemas)

				// Save the last-modified date
				.Then<Steps.CopyValueStep>()
					.Input(step => step.PlaceHolder, data => data.CurrentLastModified)
					.Output(data => data.PreviousLastModified, step => step.PlaceHolder)

				.EndWorkflow();
		}
	}
}
