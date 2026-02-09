namespace DynamicNetwork.Domain.Graph;

/// <summary>
/// Представляет временной интервал с началом и концом.
/// <para>
/// Интервал включает момент начала, но исключает момент конца [Start, End).
/// Это полуоткрытый интервал, что удобно для представления последовательных временных отрезков.
/// </para>
/// </summary>
/// <remarks>
/// Примеры использования:
/// <code>
/// var interval = new TimeInterval(0, 10); // [0, 10) - от 0 до 10, включая 0, исключая 10
/// bool covers = interval.Covers(5);       // true
/// bool overlaps = interval.Overlaps(new TimeInterval(5, 15)); // true
/// </code>
/// </remarks>
public readonly struct TimeInterval : IEquatable<TimeInterval>, IComparable<TimeInterval>
{
    /// <summary>
    /// Начало временного интервала (включительно).
    /// </summary>
    public long Start { get; }

    /// <summary>
    /// Конец временного интервала (исключительно).
    /// </summary>
    public long End { get; }

    /// <summary>
    /// Длительность интервала (End - Start).
    /// </summary>
    public long Duration => End - Start;

    /// <summary>
    /// Пустой временной интервал с нулевой длительностью.
    /// </summary>
    public static TimeInterval Empty => new TimeInterval(default, default);

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="TimeInterval"/>.
    /// </summary>
    /// <param name="start">Начало интервала.</param>
    /// <param name="end">Конец интервала.</param>
    /// <exception cref="ArgumentException">Выбрасывается, если start > end.</exception>
    public TimeInterval(long start, long end)
    {
        if (start > end)
            throw new ArgumentException("Start time must be less than or equal to end time");

        Start = start;
        End = end;
    }

    /// <summary>
    /// Проверяет, покрывает ли интервал заданную временную точку.
    /// </summary>
    /// <param name="point">Проверяемая временная точка.</param>
    /// <returns>true, если точка находится внутри интервала [Start, End).</returns>
    public bool Covers(long point) => Start <= point && point <= End;

    /// <summary>
    /// Проверяет, покрывает ли текущий интервал другой интервал полностью.
    /// </summary>
    /// <param name="other">Другой временной интервал.</param>
    /// <returns>true, если текущий интервал полностью содержит другой интервал.</returns>
    public bool Covers(TimeInterval other) => Start <= other.Start && End >= other.End;

    /// <summary>
    /// Проверяет, пересекаются ли два интервала.
    /// </summary>
    /// <param name="other">Другой временной интервал.</param>
    /// <returns>true, если интервалы имеют общую часть.</returns>
    public bool Overlaps(TimeInterval other) => Start < other.End && other.Start < End;

    /// <summary>
    /// Проверяет, пересекаются ли два интервала (синоним для Overlaps).
    /// </summary>
    public bool IntersectsWith(TimeInterval other) => Overlaps(other);

    /// <summary>
    /// Вычисляет пересечение текущего интервала с другим.
    /// </summary>
    /// <param name="other">Другой временной интервал.</param>
    /// <returns>Пересечение интервалов или null, если они не пересекаются.</returns>
    public TimeInterval? Intersection(TimeInterval other)
    {
        long start = Math.Max(Start, other.Start);
        long end = Math.Min(End, other.End);

        return start <= end ? new TimeInterval(start, end) : (TimeInterval?)null;
    }

    /// <summary>
    /// Вычисляет пересечение двух интервалов.
    /// </summary>
    /// <param name="first">Первый интервал.</param>
    /// <param name="second">Второй интервал.</param>
    /// <returns>Пересечение интервалов или null, если они не пересекаются.</returns>
    public static TimeInterval? Intersection(TimeInterval first, TimeInterval second)
    {
        long start = Math.Max(first.Start, second.Start);
        long end = Math.Min(first.End, second.End);

        return start <= end ? new TimeInterval(start, end) : (TimeInterval?)null;
    }

    /// <summary>
    /// Возвращает строковое представление интервала в формате "[Start, End)".
    /// </summary>
    public override string ToString() => $"[{Start}, {End})";

    /// <summary>
    /// Определяет равенство двух интервалов.
    /// </summary>
    public bool Equals(TimeInterval other) => Start == other.Start && End == other.End;

    /// <summary>
    /// Определяет равенство объектов.
    /// </summary>
    public override bool Equals(object? obj) => obj is TimeInterval other && Equals(other);

    /// <summary>
    /// Возвращает хэш-код интервала.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Start, End);

    /// <summary>
    /// Сравнивает текущий интервал с другим для сортировки.
    /// </summary>
    public int CompareTo(TimeInterval other)
    {
        int startComparison = Start.CompareTo(other.Start);
        return startComparison != 0 ? startComparison : End.CompareTo(other.End);
    }

    /// <summary>
    /// Оператор равенства.
    /// </summary>
    public static bool operator ==(TimeInterval left, TimeInterval right) => left.Equals(right);

    /// <summary>
    /// Оператор неравенства.
    /// </summary>
    public static bool operator !=(TimeInterval left, TimeInterval right) => !left.Equals(right);

    /// <summary>
    /// Оператор "меньше".
    /// </summary>
    public static bool operator <(TimeInterval left, TimeInterval right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Оператор "больше".
    /// </summary>
    public static bool operator >(TimeInterval left, TimeInterval right) => left.CompareTo(right) > 0;
}