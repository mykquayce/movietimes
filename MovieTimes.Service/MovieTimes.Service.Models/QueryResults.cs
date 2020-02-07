using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieTimes.Service.Models
{
	public class QueryResults : List<QueryResult>
	{
		public QueryResults() { }

		public QueryResults(IEnumerable<QueryResult> collection)
			: base(collection)
		{ }

		public int? Checksum { get; set; }
		public DateTime? DateTime { get; set; }
		public string? Json { get; set; }
		public short? QueryId { get; set; }

		public override string ToString()
			=> string.Join(Environment.NewLine, ToStrings());

		public IEnumerable<string> ToStrings()
		{
			var cinemasCount = this.Select(r => r.CinemaId!).Distinct().Count();
			var daysCount = this.Select(r => r.ShowDateTime!.Value.Date).Distinct().Count();

			var first = this.First();

			yield return (cinemasCount, daysCount) switch
			{
				(1, 1) => $"{first.CinemaName} {first.ShowDateTime:yyyy-MM-dd}",
				(1, _) => first.CinemaName!,
				(_, 1) => first.ShowDateTime!.Value.ToString("yyyy-MM-dd"),
				(_, _) => string.Empty,
			};

			foreach (var item in this)
			{
				yield return (cinemasCount, daysCount) switch
				{
					(1, 1) => item.ToString(),
					(1, _) => $"{item.ShowDateTime:yyyy-MM-dd} {item}",
					(_, 1) => $"{item.CinemaName} {item}",
					(_, _) => $"{item.CinemaName} {item.ShowDateTime:yyyy-MM-dd} {item}",
				};
			}
		}
	}
}
