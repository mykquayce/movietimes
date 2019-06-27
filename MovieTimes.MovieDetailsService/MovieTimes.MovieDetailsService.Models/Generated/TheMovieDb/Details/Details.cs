using MovieTimes.MovieDetailsService.Models.Concrete;
using System;
using System.Collections.Generic;

namespace MovieTimes.MovieDetailsService.Models.Generated.TheMovieDb.Details
{
	public class Details : MovieDetails
	{
		public bool adult { get; set; }
		public string? backdrop_path { get; set; }
		public BelongsToCollection? belongs_to_collection { get; set; }
		public int budget { get; set; }
		public IEnumerable<Genre>? genres { get; set; }
		public string? homepage { get; set; }
		public int? id { get => base.Id; set => base.Id = value; }
		public string? imdb_id
		{
			get => base.ImdbId != default ? $"tt{base.ImdbId:D}" : default;
			set => base.ImdbId = (value != default ? int.Parse(value.Substring(2)) : default);
		}
		public string? original_language { get; set; }
		public string? original_title { get; set; }
		public string? overview { get; set; }
		public double popularity { get => base.Popularity; set => base.Popularity = value; }
		public string? poster_path { get; set; }
		public IEnumerable<ProductionCompany>? production_companies { get; set; }
		public IEnumerable<ProductionCountry>? production_countries { get; set; }
		public DateTime release_date { get => base.ReleaseDate; set => base.ReleaseDate = value; }
		public int revenue { get; set; }
		public int? runtime
		{
			get => (int?)base.Runtime?.TotalMinutes;
			set => base.Runtime = value.HasValue ? TimeSpan.FromMinutes(value.Value) : default;
		}
		public IEnumerable<SpokenLanguage>? spoken_languages { get; set; }
		public string? status { get; set; }
		public string? tagline { get; set; }
		public string? title { get => base.Title; set => base.Title = value; }
		public bool video { get; set; }
		public double vote_average { get; set; }
		public int vote_count { get; set; }
	}
}
