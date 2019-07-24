using Dawn;
using MovieTimes.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieTimes.Service.Services.Concrete
{
	public class FixTitlesService : IFixTitlesService
	{
		public (string title, Formats format) Sanitize(string title)
		{
			Guard.Argument(() => title).NotNull().NotEmpty().NotWhiteSpace();

			var (sanitized, format) = ExtractFormat(title);

			return (FixArticle(sanitized), format);
		}

		public (int edi, string title, Formats format) Sanitize(Models.Generated.film film)
		{
			Guard.Argument(() => film).NotNull();

			var (title, format) = Sanitize(film.title);

			return (film.edi, title, format);
		}

		public IEnumerable<(int edi, string title, Formats format)> Sanitize(IEnumerable<Models.Generated.film> films) =>
			films.Select(Sanitize);

		private static string FixArticle(string title)
		{
			Guard.Argument(() => title).NotNull().NotEmpty().NotWhiteSpace();

			bool f(string s) => title.StartsWith(s, StringComparison.InvariantCultureIgnoreCase);

			if (f("A ")) return title[2..] + ", A";
			if (f("An ")) return title[3..] + ", An";
			if (f("The ")) return title[4..] + ", The";

			return title;
		}

		private static (string title, Formats format) ExtractFormat(string title)
		{
			Guard.Argument(() => title).NotNull().NotEmpty().NotWhiteSpace();

			bool f(string s) => title.StartsWith(s, StringComparison.InvariantCultureIgnoreCase);

			if (f("(2D) ")) return (title[5..], Formats._2d);
			if (f("(3D) ")) return (title[5..], Formats._3d);
			if (f("(4DX) ")) return (title[6..], Formats._2d | Formats._4dx);
			if (f("(4DX 3D) ")) return (title[9..], Formats._3d | Formats._4dx);
			if (f("Autism Friendly Screening: ")) return (title[27..], Formats._2d | Formats.AutismFriendlyScreening);
			if (f("(IMAX) ")) return (title[7..], Formats._2d | Formats.Imax);
			if (f("(IMAX 3-D) ")) return (title[11..], Formats._3d | Formats.Imax);
			if (f("M4J ")) return (title[4..], Formats._2d | Formats.M4J);
			if (f("(ScreenX) ")) return (title[10..], Formats._2d | Formats.ScreenX);
			if (f("Secret Unlimited Screening ")) return (title, Formats._2d | Formats.SecretUnlimitedScreening);
			if (f("(SS) ")) return (title[5..], Formats._2d | Formats.Subtitled);
			if (f("SubM4J ")) return (title[7..], Formats._2d | Formats.M4J | Formats.Subtitled);

			return (title, Formats._2d);
		}
	}
}
