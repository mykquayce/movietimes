using Helpers.Cineworld.Models;
using System;
using System.Collections.Generic;

namespace MovieTimes.Service.Models
{
	public class PersistenceData
	{
		public cinemasType? Cinemas { get; set; }
		public IEnumerable<(int edi, string title)>? EdiTitleTuples { get; set; }
		public IEnumerable<(int edi, string title, Formats format)>? EdiTitleFormatTuples { get; set; }
		public DateTime? LocalLastModified { get; set; }
		public DateTime? RemoteLastModified { get; set; }
		public IDictionary<short, string>? Queries { get; set; }
		public string? Json { get; set; }
		public ICollection<string>? Results { get; set; }
	}
}
