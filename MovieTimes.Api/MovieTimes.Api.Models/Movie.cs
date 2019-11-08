using Dawn;

namespace MovieTimes.Api.Models
{
	public readonly struct Movie
	{
		public Movie(string title, short duration)
		{
			Title = Guard.Argument(() => title).NotNull().NotEmpty().NotWhiteSpace().Value;
			Duration = Guard.Argument(() => duration).Positive().Value;
		}

		public string Title { get; }
		public short Duration { get; }

		#region Equality
		public override bool Equals(object obj) => this == obj as Movie?;
		public override int GetHashCode() => Title.GetHashCode();
		public static bool operator ==(Movie left, Movie right) => left.GetHashCode() == right.GetHashCode();
		public static bool operator !=(Movie left, Movie right) => !(left == right);
		#endregion Equality
	}
}
