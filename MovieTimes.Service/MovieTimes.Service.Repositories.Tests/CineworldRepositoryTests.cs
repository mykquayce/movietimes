using Microsoft.Extensions.Options;
using Moq;
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

		public CineworldRepositoryTests()
		{
			var settings = new Helpers.MySql.Models.DbSettings
			{
				Server = "localhost",
				Port = 3_306,
				UserId = "movietimes",
				Password = "xiebeiyoothohYaidieroh8ahchohphi",
				Database = "cineworld",
			};

			var options = Mock.Of<IOptions<Helpers.MySql.Models.DbSettings>>(o => o.Value == settings);

			_cineworldRepository = new Repositories.Concrete.CineworldRepository(options);
		}

		[Theory]
		[InlineData("2019-08-12T17:53:23Z")]
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
				actual!.LastModified);

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
