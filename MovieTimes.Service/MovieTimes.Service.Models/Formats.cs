using System;

namespace MovieTimes.Service.Models
{
	[Flags]
	public enum Formats : short
	{
		None = 0,
		_2d = 1,
		_3d = 2,
		_4dx = 4,
		AutismFriendlyScreening = 8,
		Imax = 16,
		M4J = 32,
		ScreenX = 64,
		SecretUnlimitedScreening = 128,
		Subtitled = 256,
	}
}
