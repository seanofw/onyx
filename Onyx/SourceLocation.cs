using System.Diagnostics.CodeAnalysis;

namespace Onyx
{
	public class SourceLocation : IEquatable<SourceLocation>
	{
		public string Filename { get; }
		public int Line { get; }
		public int Column { get; }
		public int Start { get; }
		public int Length { get; }

		public int End => Start + Length;

		public SourceLocation(string filename, int line, int column, int start, int length = 0)
		{
			Filename = filename;
			Line = line;
			Column = column;
			Start = start;
			Length = length;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
			=> obj is SourceLocation other && Equals(other);

		public bool Equals(SourceLocation? other)
			=> ReferenceEquals(other, null) ? false
				: ReferenceEquals(other, this) ? true
				: Filename == other.Filename
					&& Line == other.Line
					&& Column == other.Column
					&& Start == other.Start
					&& Length == other.Length;

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				hashCode = (hashCode * 65599) + Filename.GetHashCode();
				hashCode = (hashCode * 65599) + Line;
				hashCode = (hashCode * 65599) + Column;
				hashCode = (hashCode * 65599) + Start;
				hashCode = (hashCode * 65599) + Length;
				return hashCode;
			}
		}

		public static bool operator ==(SourceLocation? a, SourceLocation? b)
			=> ReferenceEquals(a, null) ? ReferenceEquals(b, null) : a.Equals(b);

		public static bool operator !=(SourceLocation? a, SourceLocation? b)
			=> ReferenceEquals(a, null) ? !ReferenceEquals(b, null) : !a.Equals(b);

		public SourceLocation Union(SourceLocation a, SourceLocation b)
		{
			if (a.Filename != b.Filename)
				return a;

			if (a.Start < b.Start)
				return new SourceLocation(a.Filename, a.Line, a.Column, a.Start, Math.Max(b.End, a.End) - a.Start);
			else
				return new SourceLocation(b.Filename, b.Line, b.Column, b.Start, Math.Max(b.End, a.End) - b.Start);
		}

		public override string ToString()
			=> $"{Filename}:{Line}:{Column}";
	}
}
