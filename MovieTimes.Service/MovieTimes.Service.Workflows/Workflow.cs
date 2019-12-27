using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Workflows
{
	public class Workflow : IWorkflow<Models.PersistenceData>
	{
		public string Id => nameof(Workflow);
		public int Version => 1;

		public void Build(IWorkflowBuilder<Models.PersistenceData> builder)
		{
			builder
				.StartWith(_ => ExecutionResult.Next())

				.Parallel()
					.Do(then =>
						then.StartWith<Steps.GetLatestLogEntryStep>()
							.Output(data => data.LocalLastModified, step => step.LogEntry != default ? step.LogEntry.LastModified : default)
							.Name("Get latest log entry from the DB")
					)
					.Do(then =>
						then.StartWith<Steps.GetListingsHeadersStep>()
							.Output(data => data.RemoteLastModified, step => step.LastModified!)
							.Name("Get the listing's last-modified date from Cineworld")
					)
				.Join()

				// If it's older than last time
				.If(data => data.LocalLastModified != default && data.RemoteLastModified <= data.LocalLastModified)
					.Do(then => then
						.StartWith(_ => ExecutionResult.Next())
						.EndWorkflow()
							.Name("No new data")
					)

				.Then<Steps.SaveLogEntryStep>()
					.Input(step => step.LastModified, data => data.RemoteLastModified)
						.Name("Add a log entry")

				.Then<Steps.GetListingsStep>()
					.Output(data => data.Cinemas, step => step.Cinemas)
						.Name("Get the listings from Cineworld")

				.Then<Steps.SaveCinemasStep>()
					.Input(step => step.Cinemas, data => data.Cinemas)
						.Name("Save the listings")

				.Then<Steps.GetQueriesStep>()
					.Output(data => data.Queries, step => step.Queries)
					.Name("Get the queries from the database")

				.ForEach(data => data.Queries)
					.Do(each => each
						.StartWith<Steps.RunQueryStep>()
							.Input(step => step.KeyValuePair, (_, context) => context.Item as KeyValuePair<short, Helpers.Cineworld.Models.Query>?)
							.Output(data => data.Json, step => default)
						/*.Then<Steps.SaveQueryResultStep>()
							.Input(step => step.KeyValuePair, (_, context) => context.Item as KeyValuePair<short, Helpers.Cineworld.Models.Query>?)
							.Input(step => step.Json, data => data.Json)
						// get the last two results
						.Then<Steps.GetLastTwoResultsStep>()
							.Input(step => step.KeyValuePair, (_, context) => context.Item as KeyValuePair<short, Helpers.Cineworld.Models.Query>?)
							.Output(data => data.Results, step => step.Results)
						.If(data => data.Results!.Count == 1)*/
					)

				.EndWorkflow();
		}
	}
}
