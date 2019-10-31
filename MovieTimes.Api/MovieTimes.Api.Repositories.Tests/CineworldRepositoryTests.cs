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
			_repository = new CineworldRepository(tracer: default, logger: default, "localhost", port: 3_306, userId: "movietimes", password: "xiebeiyoothohYaidieroh8ahchohphi", database: "cineworld");
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
				Assert.Equal(expectedId, actual.id);
				Assert.Equal(expectedName, actual.name);
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
			await foreach (var (cinemaId, cinemaName, dateTime, title) in _repository.GetShowsAsync(cinemaIds, daysOfWeek, timesOfDay, searchTerms, weekCount))
			{
				count++;

				// Assert
				Assert.InRange(cinemaId, 1, short.MaxValue);
				Assert.False(string.IsNullOrWhiteSpace(cinemaName));
				Assert.Matches(@"^\S.+\S$", cinemaName);
				Assert.InRange(dateTime, minDate, maxDate);
				Assert.Equal(0, dateTime.Second);
				Assert.Equal(0, dateTime.Millisecond);

				Assert.NotEqual(DaysOfWeek.None, daysOfWeek & Convert(dateTime.DayOfWeek));

				Assert.InRange(dateTime.Hour, 6, 18);

				Assert.False(string.IsNullOrWhiteSpace(title));
				Assert.Matches(@"^\S.+\S$", title);

				var indices = from t in searchTerms
							  select title.IndexOf(t, StringComparison.InvariantCultureIgnoreCase);

				Assert.NotEqual(-1, indices.Max());
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
