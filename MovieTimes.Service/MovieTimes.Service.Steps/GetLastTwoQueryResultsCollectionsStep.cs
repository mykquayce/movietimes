using Dawn;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.Service.Steps
{
	public class GetLastTwoQueryResultsCollectionsStep : IStepBody
	{
		private readonly Repositories.IQueriesRepository _queriesRepository;
		private readonly Services.ISerializationService _serializationService;

		public GetLastTwoQueryResultsCollectionsStep(
			Repositories.IQueriesRepository queriesRepository,
			Services.ISerializationService serializationService)
		{
			_queriesRepository = Guard.Argument(() => queriesRepository).NotNull().Value;
			_serializationService = Guard.Argument(() => serializationService).NotNull().Value;
		}

		public short? QueryId { get; set; }
		public IList<Models.QueryResults> QueryResultsCollections { get; } = new List<Models.QueryResults>();

		public async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => QueryId).NotNull().Positive();

			var collections = _queriesRepository.GetLastTwoQueryResultsCollectionsAsync(QueryId!.Value);

			await foreach (var collection in collections)
			{
				if (collection.Count == 0
					&& !(collection.Json is null))
				{
					var items = await _serializationService.DeserializeAsync<Models.QueryResults>(collection.Json!);

					foreach (var item in items)
					{
						collection.Add(item);
					}
				}

				QueryResultsCollections.Add(collection);
			}

			return ExecutionResult.Next();
		}
	}
}
