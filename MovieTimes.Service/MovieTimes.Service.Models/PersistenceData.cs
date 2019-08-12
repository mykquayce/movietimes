using System;
using System.Collections.Generic;

namespace MovieTimes.Service.Models
{
	public class PersistenceData
	{
		public Generated.cinemas? Cinemas { get; set; }
		public IEnumerable<(int edi, string title)>? EdiTitleTuples { get; set; }
		public IEnumerable<(int edi, string title, Formats format)>? EdiTitleFormatTuples { get; set; }
		public DateTime? LocalLastModified { get; set; }
		public DateTime? RemoteLastModified { get; set; }
		public IEnumerable<string>? Queries { get; set; }
	}
}
