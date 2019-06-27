using System;

namespace MovieTimes.MovieDetailsService.Models.Concrete
{
	public class MovieDetails : IMovieDetails
	{
		public int? Id { get; set; }
		public int? ImdbId { get; set; }
		public double Popularity { get; set; }
		public DateTime ReleaseDate { get; set; }
		public TimeSpan? Runtime { get; set; }
		public string? Title { get; set; }
	}
}
