using Dawn;
using MovieTimes.MovieDetailsService.Clients;
using MovieTimes.MovieDetailsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Services.Concrete
{
	public class TheMovieDbService : ITheMovieDbService
	{
		private readonly ITheMovieDbClient _theMovieDbClient;

		public TheMovieDbService(
			ITheMovieDbClient theMovieDbClient)
		{
			_theMovieDbClient = Guard.Argument(() => theMovieDbClient).NotNull().Value;
		}

		public Task<IMovieDetails> DetailsAsync(int id) =>
			_theMovieDbClient.DetailsAsync(id);

		public async IAsyncEnumerable<ISearchResult> SearchAsync(string title, params int[] years)
		{
			if (years.Length == 0)
			{
				years = new[] { DateTime.Today.Year, };
			}

			foreach (var year in years)
			{
				await foreach (var result in _theMovieDbClient.SearchAsync(title, year))
				{
					yield return result;
				}
			}
		}

		public async Task<(int? id, string title, int? imdbId, TimeSpan? runtime)> DetailsAsync(string title, params int[] years)
		{
			Guard.Argument(() => title).NotNull().NotEmpty().NotWhiteSpace();
			Guard.Argument(() => years).NotNull().Require(ii => ii.All(i => i >= 1900 && i <= DateTime.Today.Year));

			var results = await SearchAsync(title, years).ToEnumerableAsync();

			var result = results
				.OrderByDescending(r => r.Popularity)
				.FirstOrDefault();

			if (result == default)
			{
				return default;
			}

			var details = await DetailsAsync(result.Id);

			Guard.Argument(() => details.Title).NotNull().NotEmpty().NotWhiteSpace();

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
			return (details.Id, details.Title, details.ImdbId, details.Runtime);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
		}
	}

	public static class ExtensionMethods
	{
		public static async Task<IEnumerable<T>> ToEnumerableAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
		{
			var collection = new List<T>();

			var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();

			while (await asyncEnumerator.MoveNextAsync())
			{
				collection.Add(asyncEnumerator.Current);
			}

			await asyncEnumerator.DisposeAsync();

			return collection;
		}
	}
}
