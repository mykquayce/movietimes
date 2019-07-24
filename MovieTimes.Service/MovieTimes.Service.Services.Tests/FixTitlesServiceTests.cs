using MovieTimes.Service.Models;
using Xunit;

namespace MovieTimes.Service.Services.Tests
{
	public class FixTitlesServiceTests
	{
		private readonly IFixTitlesService _service;

		public FixTitlesServiceTests()
		{
			_service = new Concrete.FixTitlesService();
		}

		[Theory]
		[InlineData("(2D) Aladdin", "Aladdin", Formats._2d)]
		[InlineData("Andre Rieu 2019 Maastricht Concert Shall We Dance?", "Andre Rieu 2019 Maastricht Concert Shall We Dance?", Formats._2d)]
		[InlineData("M4J Paw Patrol: Mighty Pups - The Movie", "Paw Patrol: Mighty Pups - The Movie", Formats._2d | Formats.M4J)]
		[InlineData("SubM4J Paw Patrol: Mighty Pups - The Movie", "Paw Patrol: Mighty Pups - The Movie", Formats._2d | Formats.M4J | Formats.Subtitled)]
		[InlineData("Secret Unlimited Screening 12", "Secret Unlimited Screening 12", Formats._2d | Formats.SecretUnlimitedScreening)]
		[InlineData("(3D) The Lion King", "Lion King, The", Formats._3d)]
		[InlineData("(4DX) The Matrix : 20th Anniversary (4K)", "Matrix : 20th Anniversary (4K), The", Formats._2d | Formats._4dx)]
		[InlineData("Autism Friendly Screening: The Lion King (2019)", "Lion King (2019), The", Formats._2d | Formats.AutismFriendlyScreening)]
		[InlineData("(4DX 3D) The Lion King", "Lion King, The", Formats._3d | Formats._4dx)]
		[InlineData("(IMAX) Fast & Furious: Hobbs & Shaw", "Fast & Furious: Hobbs & Shaw", Formats._2d | Formats.Imax)]
		[InlineData("(IMAX 3-D) The Lion King", "Lion King, The", Formats._3d | Formats.Imax)]
		[InlineData("(ScreenX) Annabelle Comes Home", "Annabelle Comes Home", Formats._2d | Formats.ScreenX)]
		[InlineData("(SS) The Lion King", "Lion King, The", Formats._2d | Formats.Subtitled)]
		public void FixTitlesServiceTests_Sanitized(string title, string expectedTitle, Formats expectedFormats)
		{
			// Act
			var (actualTitle, actualFormats) = _service.Sanitize(title);

			// Arrange
			Assert.Equal(expectedTitle, actualTitle);
			Assert.Equal(expectedFormats, actualFormats);
		}
	}
}
