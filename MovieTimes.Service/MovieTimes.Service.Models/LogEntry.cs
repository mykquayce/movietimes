using System;

namespace MovieTimes.Service.Models
{
	public class LogEntry
	{
		public DateTime? LastModified { get; set; }

		public override string ToString() => $"{nameof(LastModified)}={LastModified:O}";
	}
}
