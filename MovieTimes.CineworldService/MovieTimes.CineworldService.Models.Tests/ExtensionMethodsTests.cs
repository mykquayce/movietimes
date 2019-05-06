using System;
using System.IO;
using System.Xml.Serialization;
using Xunit;

namespace MovieTimes.CineworldService.Models.Tests
{
	public class ExtensionMethodsTests
	{
		[Theory]
		[InlineData("listings.xml", 6_000_000)]
		public void ExtensionMethodsTests_Deserialize(string fileName, int maxMemoryUsage)
		{
			// Arrange
			var minDate = new DateTime(1980, 1, 1);
			var maxDate = new DateTime(2020, 1, 1);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			var before = GC.GetTotalMemory(forceFullCollection: false);

			// Act
			var cinemas = Get(fileName);

			// Arrange

			var after = GC.GetTotalMemory(forceFullCollection: false);

			// Assert
			Assert.InRange(after - before, 1, maxMemoryUsage);

			Assert.NotNull(cinemas);
			Assert.NotEmpty(cinemas.cinema);

			foreach (var cinema in cinemas.cinema)
			{
				Assert.NotNull(cinema);
				Assert.InRange(cinema.id, 1, short.MaxValue);
				Assert.False(string.IsNullOrWhiteSpace(cinema.name));
				Assert.Matches(@"^\S.+\S$", cinema.name);
				Assert.NotNull(cinema.listing);
				Assert.NotEmpty(cinema.listing);

				foreach (var film in cinema.listing)
				{
					Assert.NotNull(film);
					Assert.InRange(film.edi, 0, int.MaxValue);
					Assert.True(film.edi > 0 || film.title == "Theatre let");
					Assert.False(string.IsNullOrWhiteSpace(film.title));
					Assert.Matches(@"^\S.+\S$", film.title);
					Assert.NotNull(film.shows);
					Assert.NotEmpty(film.shows);

					foreach (var show in film.shows)
					{
						Assert.NotNull(show);
						Assert.InRange(show.time, minDate, maxDate);
						Assert.Equal(0, show.time.Second);
						Assert.Equal(0, show.time.Millisecond);
					}
				}
			}
		}

		#region Helpers
		private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(Models.Generated.cinemas));

		private static Models.Generated.cinemas Get(string fileName)
		{
			var path = Path.Combine("Data", fileName);

			using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
			{
				return (Models.Generated.cinemas)_serializer.Deserialize(stream);
			}
		}
		#endregion Helpers
	}
}
