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
			var connectionString = $"server=localhost;port=3306;user id=movietimes;password=xiebeiyoothohYaidieroh8ahchohphi;database=cineworld;";

			_repository = new CineworldRepository(tracer: default, logger: default, connectionString);
		}

		[Theory]
		[InlineData("ashton", 23, "Cineworld Ashton-under-Lyne")]
		public async Task CineworldRepositoryTests_GetCinemasAsync_ReturnsCinemas(
			string name, short expectedId, string expectedName)
		{
			// Act
			var actual = await _repository.GetCinemasAsync(name);

			// Assert
			Assert.NotNull(actual);
			Assert.NotEmpty(actual);
			Assert.Single(actual);

			var (actualId, actualName) = actual.First();

			// Assert
			Assert.Equal(expectedId, actualId);
			Assert.Equal(expectedName, actualName);
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
			var minDate = DateTime.UtcNow.Date;
			var maxDate = minDate.AddDays(100);

			// Act
			var shows = await _repository.GetShowsAsync(cinemaIds, daysOfWeek, timesOfDay, searchTerms, weekCount);

			// Assert
			Assert.NotNull(shows);
			Assert.NotEmpty(shows);

			foreach (var (cinemaId, cinemaName, dateTime, title) in shows)
			{
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
