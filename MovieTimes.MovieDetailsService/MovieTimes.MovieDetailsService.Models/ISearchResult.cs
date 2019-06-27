using System;

namespace MovieTimes.MovieDetailsService.Models
{
	public interface ISearchResult
	{
		int Id { get; }
		string? Title { get; }
		double Popularity { get; }
		DateTime ReleaseDate { get; }
	}
}
