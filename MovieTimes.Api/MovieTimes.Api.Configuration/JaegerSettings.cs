using System.Collections.Generic;

namespace MovieTimes.Api.Configuration
{
	public class JaegerSettings
	{
		public string Host { get; set; } = "localhost";
		public int Port { get; set; } = 6681;
	}
}
