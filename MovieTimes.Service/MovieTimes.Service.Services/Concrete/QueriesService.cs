using Dawn;
using Helpers.Cineworld.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace MovieTimes.Service.Services.Concrete
{
	public class QueriesService : IQueriesService
	{
		private readonly Repositories.IQueriesRepository _queriesRepository;

		public QueriesService(Repositories.IQueriesRepository queriesRepository)
		{
			_queriesRepository = Guard.Argument(() => queriesRepository).NotNull().Value;
		}

		public async IAsyncEnumerable<(short id, Query)> GetQueriesAsync()
		{
			await foreach(var (id, json) in _queriesRepository.GetQueriesAsync())
			{
				Guard.Argument(() => id).Positive();
				Guard.Argument(() => json).NotNull().NotEmpty().NotWhiteSpace().StartsWith("{");

				var query = JsonSerializer.Deserialize<Query>(json);

				yield return (id, query);
			}
		}
	}
}
