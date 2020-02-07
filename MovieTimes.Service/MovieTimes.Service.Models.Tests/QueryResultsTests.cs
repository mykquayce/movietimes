using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Service.Models.Tests
{
	public class QueryResultsTests
	{
		[Theory]
		[InlineData("QueryResults.json")]
		public async Task ToStrings(string fileName)
		{
			// Arrange
			var path = Path.Combine("Data", fileName);
			var queryResults = await Helpers.DeserializeFileAsync<Models.QueryResults>(path);

			// Act
			var lines = queryResults.ToStrings().ToList();

			// Assert
			Assert.Equal("Cineworld Ashton-under-Lyne", lines[0]);
			Assert.Equal("2020-01-20 19:45-21:38 : Like A Boss: Unlimited Screenin", lines[1]);
			Assert.Equal("2020-07-24 20:00-22:42 : Parasite (UnlimitedScreening)", lines[2]);
		}
	}
}
