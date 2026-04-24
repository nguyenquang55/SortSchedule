namespace SortSchedule.Domain.Common;

public readonly record struct HardSoftScore(int HardScore, int SoftScore) : IComparable<HardSoftScore>
{
    public static readonly HardSoftScore Zero = new(0, 0);

    public int CompareTo(HardSoftScore other)
    {
        var hardComparison = HardScore.CompareTo(other.HardScore);
        if (hardComparison != 0)
        {
            return hardComparison;
        }

        return SoftScore.CompareTo(other.SoftScore);
    }

    public static bool operator >(HardSoftScore left, HardSoftScore right) => left.CompareTo(right) > 0;

    public static bool operator <(HardSoftScore left, HardSoftScore right) => left.CompareTo(right) < 0;

    public static bool operator >=(HardSoftScore left, HardSoftScore right) => left.CompareTo(right) >= 0;

    public static bool operator <=(HardSoftScore left, HardSoftScore right) => left.CompareTo(right) <= 0;

    public static HardSoftScore operator +(HardSoftScore left, HardSoftScore right) =>
        new(left.HardScore + right.HardScore, left.SoftScore + right.SoftScore);

    public override string ToString() => $"{HardScore}hard/{SoftScore}soft";
}
