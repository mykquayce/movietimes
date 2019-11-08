using Dawn;

namespace MovieTimes.Api.Models
{
	public readonly struct Cinema
	{
		public Cinema(short id, string name)
		{
			Id = Guard.Argument(() => id).Positive().Value;
			Name = Guard.Argument(() => name).NotNull().NotEmpty().NotWhiteSpace().Value;
		}

		public short Id { get; }

		public string Name { get; }

		#region Equality
		public override bool Equals(object obj) => this == obj as Cinema?;
		public override int GetHashCode() => Id.GetHashCode();
		public static bool operator ==(Cinema left, Cinema right) => left.GetHashCode() == right.GetHashCode();
		public static bool operator !=(Cinema left, Cinema right) => !(left == right);
		#endregion Equality
	}
}
