using System;

namespace MovieTimes.Api.Models.Enums
{
	[Flags]
	public enum DaysOfWeek : byte
	{
		None = 0,

		Sunday = 1,
		Monday = 2,
		Tuesday = 4,
		Wednesday = 8,
		Thursday = 16,
		Friday = 32,
		Saturday = 64,

		Weekend = Sunday | Saturday,
		Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
	}
}
