using Microsoft.Extensions.Options;
using Moq;
using MovieTimes.Api.Models.Enums;
using MovieTimes.Api.Repositories.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MovieTimes.Api.Repositories.Tests
{
	public class CineworldRepositoryTests
	{
		private readonly ICineworldRepository _repository;

		public CineworldRepositoryTests()
		{
			var dbSettings = new Helpers.MySql.Models.DbSettings
			{
				Server = "localhost",
				Port = 3_306,
				UserId = "movietimes",
				Password = "xiebeiyoothohYaidieroh8ahchohphi",
				Database = "cineworld",
			};

			var options = Mock.Of<IOptions<Helpers.MySql.Models.DbSettings>>(o => o.Value == dbSettings);

			_repository = new CineworldRepository(tracer: default, logger: default, options);
		}

		[Theory]
		[InlineData("ashton", 23, "Cineworld Ashton-under-Lyne")]
		public async Task CineworldRepositoryTests_GetCinemasAsync_ReturnsCinemas(
			string name, short expectedId, string expectedName)
		{
			// Arrange
			var count = 0;

			// Act
			await foreach (var actual in _repository.GetCinemasAsync(name))
			{
				count++;

				// Assert
				Assert.Equal(expectedId, actual.Id);
				Assert.Equal(expectedName, actual.Name);
			}

			Assert.Equal(1, count);
		}

		[Theory]
		[InlineData(new[] { (short)23, }, DaysOfWeek.Monday, TimesOfDay.Morning | TimesOfDay.Afternoon, new[] { "dark", "phoenix", }, 0)]
		public async Task CineworldRepositoryTests_GetShowsAsync(
			ICollection<short> cinemaIds,
			DaysOfWeek daysOfWeek,
			TimesOfDay timesOfDay,
			ICollection<string> searchTerms,
			int weekCount)
		{
			// Arrange
			var count = 0;
			var minDate = DateTime.UtcNow.Date;
			var maxDate = minDate.AddDays(100);

			// Act
			await foreach (var show in _repository.GetShowsAsync(cinemaIds, daysOfWeek, timesOfDay, searchTerms, weekCount))
			{
				count++;

				// Assert
				Assert.InRange(show.Cinema.Id, 1, short.MaxValue);
				Assert.False(string.IsNullOrWhiteSpace(show.Cinema.Name));
				Assert.Matches(@"^\S.+\S$", show.Cinema.Name);
				Assert.InRange(show.DateTime, minDate, maxDate);
				Assert.Equal(0, show.DateTime.Second);
				Assert.Equal(0, show.DateTime.Millisecond);

				Assert.NotEqual(DaysOfWeek.None, daysOfWeek & Convert(show.DateTime.DayOfWeek));

				Assert.InRange(show.DateTime.Hour, 6, 18);

				Assert.False(string.IsNullOrWhiteSpace(show.Movie.Title));
				Assert.Matches(@"^\S.+\S$", show.Movie.Title);

				var indices = from t in searchTerms
							  select show.Movie.Title.IndexOf(t, StringComparison.InvariantCultureIgnoreCase);

				Assert.NotEqual(-1, indices.Max());

				Assert.InRange(show.Movie.Duration, 10, short.MaxValue);
			}

			Assert.InRange(count, 1, int.MaxValue);
		}

		private static DaysOfWeek Convert(DayOfWeek dayOfWeek) => dayOfWeek.ConvertEnum<DayOfWeek, DaysOfWeek>();
	}

	public static class ExtensionMethods
	{
		public static TTarget ConvertEnum<TSource, TTarget>(this TSource source)
			where TSource : struct, Enum
			where TTarget : struct, Enum
		{
			var s = source.ToString("G");

			var success = Enum.TryParse<TTarget>(s, ignoreCase: true, out var result);

			if (success)
			{
				return result;
			}

			var expectedValues = Enum.GetNames(typeof(TTarget));
			var expectedValuesString = string.Join(", ", expectedValues);

			throw new ArgumentOutOfRangeException(
					nameof(source),
					source.ToString(),
					$"Unexpected value of {nameof(source)}: {source}.  Expected values are: {expectedValuesString}")
			{
				Data =
				{
					[nameof(source)] = source,
					[nameof(expectedValues)] = expectedValuesString,
				},
			};
		}
	}
}
