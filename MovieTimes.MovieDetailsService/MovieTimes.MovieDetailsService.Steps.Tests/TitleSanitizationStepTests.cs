using MovieTimes.MovieDetailsService.Models;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.MovieDetailsService.Steps.Tests
{
	public class TitleSanitizationStepTests
	{
		private readonly TitleSanitizationStep _step;

		public TitleSanitizationStepTests()
		{
			_step = new TitleSanitizationStep();
		}

		[Theory]
		[InlineData("Jaws (Re 2012)", "Jaws (Re 2012)", Formats._2d)]
		[InlineData("(2D) Toy Story 4", "Toy Story 4", Formats._2d)]
		[InlineData("(3D) Toy Story 4", "Toy Story 4", Formats._3d)]
		[InlineData("(IMAX 3-D) A Beautiful Planet", "Beautiful Planet, A", Formats._3d | Formats.Imax)]
		[InlineData("(IMAX) Fast & Furious: Hobbs & Shaw", "Fast & Furious: Hobbs & Shaw", Formats._2d | Formats.Imax)]
		[InlineData("(SS) X-Men: Dark Phoenix", "X-Men: Dark Phoenix", Formats._2d | Formats.Subtitled)]
		[InlineData("(ScreenX) Godzilla: King Of The Monsters", "Godzilla: King Of The Monsters", Formats._2d | Formats.ScreenX)]
		[InlineData("(4DX) The Matrix : 20th Anniversary (4K)", "Matrix : 20th Anniversary (4K), The", Formats._2d | Formats._4dx)]
		[InlineData("(4DX 3D) Spider-Man : FAR FROM HOME", "Spider-Man : FAR FROM HOME", Formats._3d | Formats._4dx)]
		[InlineData("M4J Missing Link", "Missing Link", Formats._2d | Formats.MoviesForJuniors)]
		[InlineData("Autism Friendly Screening: Toy Story 4", "Toy Story 4", Formats._2d | Formats.AutismFriendlyScreening)]
		[InlineData("Dementia Friendly Screening Rocketman", "Rocketman", Formats._2d | Formats.DementiaFriendlyScreening)]
		[InlineData("SubM4J Dumbo", "Dumbo", Formats._2d | Formats.Subtitled | Formats.MoviesForJuniors)]
		[InlineData("Yesterday : Unlimited Screening", "Yesterday", Formats._2d | Formats.UnlimitedScreening)]
		public async Task TitleSanitizationStepTests_BehavesPredictably(
			string title, string sanitized, Formats formats)
		{
			// Arrange
			_step.Movie = new PersistenceData.Movie { Title = title, };

			// Act
			await _step.RunAsync(context: default);

			// Assert
			Assert.Equal(sanitized, _step.Movie.Sanitized);
			Assert.Equal(formats, _step.Movie.Formats);
		}
	}
}
