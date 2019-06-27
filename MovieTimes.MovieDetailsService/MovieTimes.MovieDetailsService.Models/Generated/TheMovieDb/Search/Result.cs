using MovieTimes.MovieDetailsService.Models.Concrete;
using System;
using System.Collections.Generic;

namespace MovieTimes.MovieDetailsService.Models.Generated.TheMovieDb.Search
{
	public class Result : SearchResult
	{
		public int vote_count { get; set; }
		public int id { get => base.Id; set => base.Id = value; }
		public bool video { get; set; }
		public double vote_average { get; set; }
		public string? title { get => base.Title; set => base.Title = value; }
		public double popularity { get => base.Popularity; set => base.Popularity = value; }
		public string? poster_path { get; set; }
		public string? original_language { get; set; }
		public string? original_title { get; set; }
		public IEnumerable<int> genre_ids { get; } = new List<int>();
		public string? backdrop_path { get; set; }
		public bool adult { get; set; }
		public string? overview { get; set; }
		public DateTime release_date { get => base.ReleaseDate; set => base.ReleaseDate = value; }
	}
}
