using System;

namespace MovieTimes.Service.Models
{
	public class QueryResult
	{
		public short? CinemaId { get; set; }
		public string? CinemaName { get; set; }
		public string? FilmTitle { get; set; }
		public short? FilmLength { get; set; }
		public DateTime? ShowDateTime { get; set; }

		public override string ToString()
		{
			if (string.IsNullOrWhiteSpace(CinemaName)) throw new ArgumentNullException(nameof(CinemaName));
			if (string.IsNullOrWhiteSpace(FilmTitle)) throw new ArgumentNullException(nameof(FilmTitle));
			if (ShowDateTime is null) throw new ArgumentNullException(nameof(ShowDateTime));

			var endTime = FilmLength.HasValue
				? ShowDateTime!.Value.AddMinutes(30).AddMinutes(FilmLength!.Value).TimeOfDay
				: (TimeSpan?)default;

			var endTimeString = endTime?.ToString(@"hh\:mm") ?? "     ";

			return $"{ShowDateTime:HH:mm}-{endTimeString} : {FilmTitle}";
		}
	}
}
