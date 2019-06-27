using System;

namespace MovieTimes.MovieDetailsService.Models
{
	[Flags]
	public enum Formats : ushort
	{
		None = 0,
		_2d = 1,
		_3d = 2,
		_4dx = 4,
		AutismFriendlyScreening = 8,
		DementiaFriendlyScreening = 16,
		Imax = 32,
		MoviesForJuniors = 64,
		ScreenX = 128,
		Subtitled = 256,
		UnlimitedScreening = 512,
	}
}
