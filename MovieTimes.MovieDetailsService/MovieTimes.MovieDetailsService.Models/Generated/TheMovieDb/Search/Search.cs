using System.Collections.Generic;

namespace MovieTimes.MovieDetailsService.Models.Generated.TheMovieDb.Search
{
	public class Search
	{
		public int page { get; set; }
		public int total_results { get; set; }
		public int total_pages { get; set; }
		public IEnumerable<Result> results { get; set; }
	}
}
