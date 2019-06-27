using System;

namespace MovieTimes.MovieDetailsService.Models
{
	public interface IMovieDetails
	{
		int? Id { get; }
		int? ImdbId { get; }
		double Popularity { get; }
		DateTime ReleaseDate { get; }
		TimeSpan? Runtime { get; set; }
		string? Title { get; }
	}
}
