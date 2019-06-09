namespace MovieTimes.Api.Configuration
{
	public class DbSettings
	{
		public string? Server { get; set; }
		public int Port { get; set; }
		public string? UserId { get; set; }
		public string? Password { get; set; }
		public string? Database { get; set; }

		public string ConnectionString => $"server={Server};port={Port:D};user id={UserId};password={Password};database={Database};";
	}
}
