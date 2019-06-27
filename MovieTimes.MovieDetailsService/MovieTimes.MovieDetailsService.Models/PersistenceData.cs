using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MovieTimes.MovieDetailsService.Models
{
	public class PersistenceData
	{
		public class Movie
		{
			public int Edi { get; set; }
			public string? Title { get; set; }
			public string? Sanitized { get; set; }
			public Formats? Formats { get; set; }
			public int? Id { get; set; }
			public int? ImdbId { get; set; }
			public TimeSpan? Runtime { get; set; }
		}

		private readonly ConcurrentDictionary<int, Movie> _dictionary = new ConcurrentDictionary<int, Movie>();

		public ICollection<Movie> Movies
		{
			get => _dictionary.Values;
			set
			{
				foreach (var movie in value)
				{
					if (!_dictionary.TryAdd(movie.Edi, movie))
					{
						_dictionary[movie.Edi] = movie;
					}
				}
			}
		}

		public Movie MovieItem
		{
			get => throw new NotImplementedException();
			set
			{
				if (_dictionary.TryAdd(value.Edi, value))
				{
					return;
				}

				if (value.Title != default) _dictionary[value.Edi].Title = value.Title;
				if (value.Sanitized != default) _dictionary[value.Edi].Sanitized = value.Sanitized;
				if (value.Formats != default) _dictionary[value.Edi].Formats = value.Formats;
				if (value.Id != default) _dictionary[value.Edi].Id = value.Id;
				if (value.ImdbId != default) _dictionary[value.Edi].ImdbId = value.ImdbId;
				if (value.Runtime != default) _dictionary[value.Edi].Runtime = value.Runtime;
			}
		}
	}
}
