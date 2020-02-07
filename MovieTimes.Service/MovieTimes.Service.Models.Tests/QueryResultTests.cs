using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Models.Tests
{
	public class QueryResultTests
	{
		[Theory]
		[InlineData("QueryResults.json")]
		public async Task Deserialize(string fileName)
		{
			// Arrange
			var path = Path.Combine("Data", fileName);

			// Act
			var queryResults = await Helpers.DeserializeFileAsync<Models.QueryResults>(path);

			// Assert
			Assert.NotEmpty(queryResults);
			Assert.Equal(2, queryResults.Count);

			Assert.Equal((short?)23, queryResults[0].CinemaId);
			Assert.Equal("Cineworld Ashton-under-Lyne", queryResults[0].CinemaName);
			Assert.Equal("Like A Boss: Unlimited Screenin", queryResults[0].FilmTitle);
			Assert.Equal((short?)83, queryResults[0].FilmLength);
			Assert.Equal(new DateTime(2020, 1, 20, 19, 45, 0, DateTimeKind.Local), queryResults[0].ShowDateTime);

			Assert.Equal((short?)23, queryResults[1].CinemaId);
			Assert.Equal("Cineworld Ashton-under-Lyne", queryResults[1].CinemaName);
			Assert.Equal("Parasite (UnlimitedScreening)", queryResults[1].FilmTitle);
			Assert.Equal((short?)132, queryResults[1].FilmLength);
			Assert.Equal(new DateTime(2020, 7, 24, 20, 0, 0, DateTimeKind.Local), queryResults[1].ShowDateTime);
		}
	}
}
