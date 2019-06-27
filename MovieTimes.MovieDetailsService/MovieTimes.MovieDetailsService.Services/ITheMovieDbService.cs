using MovieTimes.MovieDetailsService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTimes.MovieDetailsService.Services
{
	public interface ITheMovieDbService
	{
		Task<IMovieDetails> DetailsAsync(int id);
		IAsyncEnumerable<ISearchResult> SearchAsync(string title, params int[] years);
		Task<(int? id, string title, int? imdbId, TimeSpan? runtime)> DetailsAsync(string title, params int[] years);
	}
}
