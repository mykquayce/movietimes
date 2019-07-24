namespace MovieTimes.Service.Configuration
{
	public class DbSettings
	{
		public string? Database { get; set; }
		public string? Password { get; set; } = "guest";
		public int Port { get; set; } = 3_306;
		public string Server { get; set; } = "localhost";
		public string? UserId { get; set; } = "guest";

	}
}
