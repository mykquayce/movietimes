using Dawn;
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
		private static readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(15);

		public string Id => nameof(Workflow);
		public int Version => 1;

		public void Build(IWorkflowBuilder<Models.PersistenceData> builder)
		{
			_ = builder
				.StartWith(_ => ExecutionResult.Next())

				.Parallel()
					.Do(then => then
						.StartWith<Steps.GetLatestLogEntryStep>()
							.Output(data => data.LocalLastModified, step => step.LogEntry != default ? step.LogEntry.LastModified : default)
							.OnError(WorkflowErrorHandling.Retry, _retryInterval)
							.Name("Get latest log entry from the DB")
					)
					.Do(then => then
						.StartWith<Steps.GetHeadersStep>()
							.Output(data => data.RemoteLastModified, step => step.LastModified!)
							.Name("Get the listing's last-modified date from Cineworld")
					)
				.Join()
					.Name("Check if work needs to be done")

				// If it's older than last time
				.If(data => data.LocalLastModified != default && data.RemoteLastModified <= data.LocalLastModified)
					.Do(then => then
						.StartWith(_ => ExecutionResult.Next())
						.EndWorkflow()
							.Name("Ending: no new data")
					)

				.Then<Steps.SaveLogEntryStep>()
					.Input(step => step.LastModified, data => data.RemoteLastModified)
					.Name("Add a log entry")

				.Parallel()
					.Do(then => then
						.StartWith<Steps.GetListingsStep>()
							.Output(data => data.Cinemas, step => step.Cinemas)
							.Name("Get the listings from Cineworld")

						.Then<Steps.SaveCinemasStep>()
							.Input(step => step.Cinemas, data => data.Cinemas)
							.Name("Save the listings")
					)
					.Do(then => then
						.StartWith<Steps.GetFilmsStep>()
							.Output(data => data.Films, step => step.Films)
							.Name("Get the film lengths")
					)
				.Join()

				.Then<Steps.SaveFilmLengthsStep>()
					.Input(step => step.Films, data => data.Films)
					.Name("Save the film lengths")

				.Then<Steps.GetQueriesStep>()
					.Output(data => data.Queries, step => step.Queries)
					.Name("Get the queries from the database")

				.ForEach(data => data.Queries)
					.Do(each => each
						.StartWith<Steps.RunQueryStep>()
							.Input(step => step.KeyValuePair, (_, context) => context.Item as KeyValuePair<short, Helpers.Cineworld.Models.Query>?)
							.Output(data => data.QueryResults, step => step.Results)
							.Name("Run each query on the new data")
					)

				.ForEach(data => data.QueryResultsBag)
					.Do(each => each
						.StartWith<Steps.SaveQueryResultsStep>()
							.Input(step => step.QueryResults, (data, context) => context.Item as Models.QueryResults)
					)

				.ForEach(data => data.QueryIds)
					.Do(each => each
						.StartWith<Steps.GetLastTwoQueryResultsCollectionsStep>()
							.Input(step => step.QueryId, (_, context) => context.Item as short?)
							.Output(data => data.QueryResultsCollections, step => step.QueryResultsCollections)
					)

				.ForEach(data => data.QueryResultsCollectionBag)
					.Do(each => each
						.StartWith<Steps.BuildMessageStep>()
							.Input(step => step.QueryResultsCollection, (data, context) => context.Item as IList<Models.QueryResults>)
							.Output(data => data.Message, step => step.Message)
					)

				.ForEach(data => data.MessagesBag)
					.Do(each => each
						.StartWith<Steps.SendMessageToDiscordStep>()
							.Input(step => step.Message, (_, context) => context.Item as string)
					)

				.EndWorkflow();
		}
	}
}
