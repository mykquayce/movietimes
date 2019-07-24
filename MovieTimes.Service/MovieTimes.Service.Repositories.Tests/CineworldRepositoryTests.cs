using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xunit;

namespace MovieTimes.Service.Repositories.Tests
{
	public class CineworldRepositoryTests
	{
		private readonly Repositories.ICineworldRepository _cineworldRepository;
		private readonly XmlSerializer _xmlSerializer;

		public CineworldRepositoryTests()
		{
			var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder
			{
				Server = "localhost",
				Port = 3_306,
				UserID = "movietimes",
				Password = "xiebeiyoothohYaidieroh8ahchohphi",
				Database = "cineworld",
			};

			_cineworldRepository = new Repositories.Concrete.CineworldRepository(builder.ConnectionString);

			_xmlSerializer = new XmlSerializer(typeof(Models.Generated.cinemas));
		}

		[Theory]
		[InlineData("2019-07-21T12:33:51Z")]
		public async Task CineworldRepositoryTests_Logs(string lastModifiedString)
		{
			// Arrange
			var lastModified = DateTime.Parse(lastModifiedString, styles: DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
			var logEntry = new Models.LogEntry { LastModified = lastModified, };

			// Act
			await _cineworldRepository.PurgeOldLogsAsync(lastModified);
			await _cineworldRepository.LogAsync(logEntry);

			var actual = await _cineworldRepository.GetLatestLogEntryAsync();

			// Assert
			Assert.Equal(
				lastModified,
				actual.LastModified);

			// Act
			await _cineworldRepository.PurgeOldLogsAsync(lastModified);
		}

		[Theory]
		[InlineData("listings.xml")]
		public async Task CineworldRepositoryTests_SaveCinemasAsync(string filename)
		{
			// Arrange
			var path = Path.Combine("Data", filename);
			var cinemas = DeserializeFile<Models.Generated.cinemas>(path);

			// Act
			await _cineworldRepository.SaveCinemasAsync(cinemas);
		}

		private static T DeserializeFile<T>(string path)
		{
			var serializer = new XmlSerializer(typeof(T));
			using var reader = new StreamReader(path);
			return (T)serializer.Deserialize(reader);
		}
	}
}
