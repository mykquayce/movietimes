using System;

namespace MovieTimes.MovieDetailsService.Models.Concrete
{
	public class SearchResult : ISearchResult
	{
		public int Id { get; set; }
		public string? Title { get; set; }
		public double Popularity { get; set; }
		public DateTime ReleaseDate { get; set; }
	}
}
