namespace ChronoNet.Domain;
public readonly struct TimeInterval : IEquatable<TimeInterval>, IComparable<TimeInterval>
{
    public long Start { get; }
    public long End { get; }
    public long Duration => End - Start;

    public TimeInterval(long start, long end)
    {
        if (start > end)
            throw new ArgumentException("Start time must be less than or equal to end time");

        Start = start;
        End = end;
    }

    public bool Covers(long point) => Start <= point && point <= End;
    public bool Covers(TimeInterval other) => Start <= other.Start && End >= other.End;
    public bool Overlaps(TimeInterval other) => Start < other.End && other.Start < End;

    public bool IntersectsWith(TimeInterval other) => Overlaps(other);
    public TimeInterval? Intersection(TimeInterval other)
    {
        long start = Math.Max(Start, other.Start);
        long end = Math.Min(End, other.End);

        return start <= end ? new TimeInterval(start, end) : (TimeInterval?)null;
    }

    public override string ToString() => $"[{Start}, {End})";

    public bool Equals(TimeInterval other) => Start == other.Start && End == other.End;
    public override bool Equals(object? obj) => obj is TimeInterval other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Start, End);

    public int CompareTo(TimeInterval other)
    {
        int startComparison = Start.CompareTo(other.Start);
        return startComparison != 0 ? startComparison : End.CompareTo(other.End);
    }

    public static bool operator ==(TimeInterval left, TimeInterval right) => left.Equals(right);
    public static bool operator !=(TimeInterval left, TimeInterval right) => !left.Equals(right);
    public static bool operator <(TimeInterval left, TimeInterval right) => left.CompareTo(right) < 0;
    public static bool operator >(TimeInterval left, TimeInterval right) => left.CompareTo(right) > 0;
}