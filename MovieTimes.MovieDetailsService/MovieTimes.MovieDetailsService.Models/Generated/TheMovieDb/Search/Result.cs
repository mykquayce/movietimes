using System.Collections.Generic;

namespace MovieTimes.MovieDetailsService.Models.Generated.TheMovieDb.Search
{
	public class Result
	{
		public int vote_count { get; set; }
		public int id { get; set; }
		public bool video { get; set; }
		public double vote_average { get; set; }
		public string title { get; set; }
		public double popularity { get; set; }
		public string poster_path { get; set; }
		public string original_language { get; set; }
		public string original_title { get; set; }
		public IEnumerable<int> genre_ids { get; set; }
		public string backdrop_path { get; set; }
		public bool adult { get; set; }
		public string overview { get; set; }
		public string release_date { get; set; }
	}
}
