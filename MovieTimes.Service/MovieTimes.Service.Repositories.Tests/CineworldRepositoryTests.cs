using Helpers.Cineworld.Models;
using Helpers.Cineworld.Models.Generated;
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
		private static readonly XmlSerializerFactory _xmlSerializerFactory = new XmlSerializerFactory();
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
		public async Task GetLatestLogEntryAsync(string lastModifiedString)
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
		[InlineData("cinemas.xml")]
		public async Task SaveCinemasAsync(string filename)
		{
			// Arrange
			var path = Path.Combine("Data", filename);
			var cinemas = DeserializeFile<CinemasType>(path);

			// Assert
			Assert.NotNull(cinemas);
			Assert.NotNull(cinemas.Cinema);
			Assert.NotEmpty(cinemas.Cinema);

			try
			{
				// Act
				await _cineworldRepository.SaveCinemasAsync(cinemas.Cinema);
			}
			catch (Exception ex)
			{
				// Assert
				Assert.True(false, ex.Message);
			}
		}

		[Theory]
		[InlineData(23)]
		public async Task RunQueryAsync(short cinemaId)
		{
			var query = new Query
			{
				CinemaIds = { cinemaId, },
			};

			var count = 0;
			var results = _cineworldRepository.RunQueryAsync(query);

			await foreach (var result in results)
			{
				count++;
			}

			Assert.InRange(count, 1, int.MaxValue);
		}

		private static T DeserializeFile<T>(string path)
		{
			var serializer = _xmlSerializerFactory.CreateSerializer(typeof(T));
			using var reader = new StreamReader(path);
			return (T)serializer.Deserialize(reader);
		}
	}
}
