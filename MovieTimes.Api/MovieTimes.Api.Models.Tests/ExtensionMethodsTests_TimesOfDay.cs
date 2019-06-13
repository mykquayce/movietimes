using MovieTimes.Api.Models.Enums;
using MovieTimes.Api.Models.Helpers;
using System.Linq;
using Xunit;

namespace MovieTimes.Api.Models.Tests
{
	public class ExtensionMethodsTests_TimesOfDay
	{
		[Theory]
		[InlineData(TimesOfDay.Night, 0, 1, 2, 3, 4, 5)]
		[InlineData(TimesOfDay.Morning, 6, 7, 8, 9, 10, 11)]
		[InlineData(TimesOfDay.Afternoon, 12, 13, 14, 15, 16, 17)]
		[InlineData(TimesOfDay.Evening, 18, 19, 20, 21, 22, 23)]
		[InlineData(TimesOfDay.AM, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)]
		[InlineData(TimesOfDay.PM, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23)]
		[InlineData(TimesOfDay.AllDay, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23)]
		public void ExtensionMethodsTests_TimesOfDay_ToHours_BehavesPredictably(
			TimesOfDay timesOfDay, params int[] expected)
		{
			Assert.Equal(
				expected.Select(i => (byte)i),
				timesOfDay.ToHours());
		}
	}
}
