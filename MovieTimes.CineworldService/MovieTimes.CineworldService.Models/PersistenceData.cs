using OpenTracing;
using System;
using System.Data;

namespace MovieTimes.CineworldService.Models
{
	public class PersistenceData
	{
		public IScope? Scope { get; set; }
		public ConnectionState ConnectionState { get; set; }
		public DateTime? LocalLastModified { get; set; }
		public DateTime? RemoteLastModified { get; set; }
		public Generated.cinemas? Cinemas { get; set; }
	}
}
