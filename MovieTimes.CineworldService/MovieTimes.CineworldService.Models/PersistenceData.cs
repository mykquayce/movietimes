using OpenTracing;
using System;
using System.Data;

namespace MovieTimes.CineworldService.Models
{
	public class PersistenceData
	{
		public ISpan? Span { get; set; }
		public ConnectionState ConnectionState { get; set; }
		public DateTime? CurrentLastModified { get; set; }
		public DateTime? PreviousLastModified { get; set; }
		public Generated.cinemas? Cinemas { get; set; }
	}
}
