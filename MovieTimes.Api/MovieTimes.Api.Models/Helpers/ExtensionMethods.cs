using MovieTimes.Api.Models.Enums;
using System.Collections.Generic;

namespace MovieTimes.Api.Models.Helpers
{
	public static class ExtensionMethods
	{
		public static IEnumerable<byte> ToHours(this TimesOfDay timesOfDay)
		{
			if ((timesOfDay & TimesOfDay.Night) != 0)
			{
				yield return 0;
				yield return 1;
				yield return 2;
				yield return 3;
				yield return 4;
				yield return 5;
			}

			if ((timesOfDay & TimesOfDay.Morning) != 0)
			{
				yield return 6;
				yield return 7;
				yield return 8;
				yield return 9;
				yield return 10;
				yield return 11;
			}

			if ((timesOfDay & TimesOfDay.Afternoon) != 0)
			{
				yield return 12;
				yield return 13;
				yield return 14;
				yield return 15;
				yield return 16;
				yield return 17;
			}

			if ((timesOfDay & TimesOfDay.Evening) != 0)
			{
				yield return 18;
				yield return 19;
				yield return 20;
				yield return 21;
				yield return 22;
				yield return 23;
			}
		}
	}
}
