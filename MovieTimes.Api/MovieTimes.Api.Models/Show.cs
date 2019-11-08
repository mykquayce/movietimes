using Dawn;
using System;

namespace MovieTimes.Api.Models
{
	public readonly struct Show
	{
		public Show(short cinemaId, string cinemaName, string title, short duration, DateTime dateTime)
			: this(new Cinema(cinemaId, cinemaName), new Movie(title, duration), dateTime)
		{ }

		public Show(Cinema cinema, Movie movie, DateTime dateTime)
		{
			Cinema = cinema;
			Movie = movie;
			DateTime = Guard.Argument(() => dateTime).NotDefault().Value;
		}

		public Cinema Cinema { get; }
		public Movie Movie { get; }
		public DateTime DateTime { get; }

		#region Equality
		public override bool Equals(object obj) => this == obj as Show?;
		public override int GetHashCode()
		{
			unchecked
			{
				return Cinema.GetHashCode() * Movie.GetHashCode() * DateTime.GetHashCode();
			}
		}

		public static bool operator ==(Show left, Show right) => left.GetHashCode() == right.GetHashCode();
		public static bool operator !=(Show left, Show right) => !(left == right);
		#endregion Equality
	}
}
