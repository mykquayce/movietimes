using Dawn;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace MovieTimes.MovieDetailsService.Steps
{
	public class TitleSanitizationStep : IStepBody
	{
		public Models.PersistenceData.Movie? Movie { get; set; }

		public Task<ExecutionResult> RunAsync(IStepExecutionContext context)
		{
			Guard.Argument(() => Movie).NotNull();
#pragma warning disable CS8602, CS8604
			Guard.Argument(() => Movie.Title).NotNull().NotEmpty().NotWhiteSpace();

			(Movie.Sanitized, Movie.Formats) = Prefixes(Movie.Title);
#pragma warning restore CS8602, CS8604

			if (Movie.Sanitized.EndsWith(" : Unlimited Screening"))
			{
				Movie.Sanitized = Movie.Sanitized[0..^22];
				Movie.Formats |= Models.Formats.UnlimitedScreening;
			}

			Movie.Sanitized = Articles(Movie.Sanitized);

			return Task.FromResult(ExecutionResult.Next());
		}

		private static (string sanitized, Models.Formats formats) Prefixes(string title)
		{
			Guard.Argument(() => title).NotNull().NotEmpty().NotWhiteSpace();

			if (title.StartsWith("(2D) "))
			{
				return (title[5..], Models.Formats._2d);
			}

			if (title.StartsWith("(3D) "))
			{
				return (title[5..], Models.Formats._3d);
			}

			if (title.StartsWith("(IMAX) "))
			{
				return (title[7..], Models.Formats._2d | Models.Formats.Imax);
			}

			if (title.StartsWith("(IMAX 3-D) "))
			{
				return (title[11..], Models.Formats._3d | Models.Formats.Imax);
			}

			if (title.StartsWith("(SS) "))
			{
				return (title[5..], Models.Formats._2d | Models.Formats.Subtitled);
			}

			if (title.StartsWith("(ScreenX) "))
			{
				return (title[10..], Models.Formats._2d | Models.Formats.ScreenX);
			}

			if (title.StartsWith("(4DX) "))
			{
				return (title[6..], Models.Formats._2d | Models.Formats._4dx);
			}

			if (title.StartsWith("(4DX 3D) "))
			{
				return (title[9..], Models.Formats._3d | Models.Formats._4dx);
			}

			if (title.StartsWith("M4J "))
			{
				return (title[4..], Models.Formats._2d | Models.Formats.MoviesForJuniors);
			}

			if (title.StartsWith("Autism Friendly Screening: "))
			{
				return (title[27..], Models.Formats._2d | Models.Formats.AutismFriendlyScreening);
			}

			if (title.StartsWith("Dementia Friendly Screening "))
			{
				return (title[28..], Models.Formats._2d | Models.Formats.DementiaFriendlyScreening);
			}

			if (title.StartsWith("SubM4J "))
			{
				return (title[7..], Models.Formats._2d | Models.Formats.Subtitled | Models.Formats.MoviesForJuniors);
			}

			return (title, Models.Formats._2d);
		}

		private static string Articles(string title)
		{
			Guard.Argument(() => title).NotNull().NotEmpty().NotWhiteSpace();

			if (title.StartsWith("A "))
			{
				return title[2..] + ", A";
			}

			if (title.StartsWith("An "))
			{
				return title[3..] + ", An";
			}

			if (title.StartsWith("The "))
			{
				return title[4..] + ", The";
			}

			return title;
		}
	}
}
