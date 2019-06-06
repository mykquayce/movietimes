﻿namespace MovieTimes.MovieDetailsService.Configuration
{
	public class DbSettings
	{
		public string Server { get; set; }
		public int Port { get; set; }
		public string UserId { get; set; }
		public string Password { get; set; }
		public string Database { get; set; }

		public override string ToString() =>
			$"server={Server};port={Port:D};user id={UserId};password={Password};database={Database};";
	}
}
