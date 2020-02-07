using System;

namespace MovieTimes.Api.Models
{
	public class Flattened
	{
		private static readonly TimeSpan _durationPadding = TimeSpan.FromMinutes(30);

		public string? Cinema { get; set; }
		public short? Duration { get; set; }
		public string? Movie { get; set; }
		public DateTime? DateTime { get; set; }
		public TimeSpan? End => DateTime?.TimeOfDay + TimeSpan.FromMinutes(Duration ?? 0) + _durationPadding;
	}
}
