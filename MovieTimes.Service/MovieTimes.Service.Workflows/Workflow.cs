﻿using System;
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
					.Name("before")

				.Parallel()
					.Do(then =>
						then.StartWith<Steps.GetLatestLogEntryStep>()
							.Output(data => data.LocalLastModified, step => step.LogEntry != default ? step.LogEntry.LastModified : default)
					)
					.Do(then =>
						then.StartWith<Steps.GetListingsHeadersStep>()
							.Output(data => data.RemoteLastModified, step => step.LastModified!)
					)
				.Join()

				// If it's older than last time
				.If(data => data.LocalLastModified != default && data.RemoteLastModified <= data.LocalLastModified)
					.Do(then => then
						.StartWith(_ => ExecutionResult.Next())
						.EndWorkflow()
					)

				.Then<Steps.SaveLogEntryStep>()
					.Input(step => step.LastModified, data => data.RemoteLastModified)

				.Then<Steps.GetListingsStep>()
					.Output(data => data.Cinemas, step => step.Cinemas)

				.Then<Steps.SaveCinemasStep>()
					.Input(step => step.Cinemas, data => data.Cinemas)

				.Then<Steps.GetQueriesStep>()
					.Output(data => data.Queries, step => step.Queries)

				.ForEach(data => data.Queries)
					.Do(each => each
						.StartWith<Steps.RunQueryStep>()
							.Input(step => step.KeyValuePair, (_, context) => context.Item as KeyValuePair<int, string>?)
							.Output(data => data.Json, step => step.Json)
						.Then<Steps.SaveQueryResultStep>()
							.Input(step => step.KeyValuePair, (_, context) => context.Item as KeyValuePair<int, string>?)
							.Input(step => step.Json, data => data.Json)
					)

				/*.Then<Steps.GetTitlesStep>()
					.Input(step => step.Cinemas, data => data.Cinemas)
					.Output(data => data.EdiTitleTuples, step => step.EdiTitleTuples)

				.Then<Steps.FixTitlesStep>()
					.Input(step => step.EdiTitleTuples, data => data.EdiTitleTuples)
					.Output(data => data.EdiTitleFormatTuples, step => step.EdiTitleFormatTuples)

				.Then<Steps.SaveTitlesStep>()
					.Input(step => step.EdiTitleFormatTuples, data => data.EdiTitleFormatTuples)*/

				.EndWorkflow();
		}
	}
}
