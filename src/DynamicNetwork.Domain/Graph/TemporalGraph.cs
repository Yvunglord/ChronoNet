namespace DynamicNetwork.Domain.Graph;

/// <summary>
/// Представляет временной граф - снимок топологии сети в интервал постоянства структуры.
/// <para>
/// Временной граф описывает, какие связи между узлами активны в заданный временной интервал.
/// Это позволяет моделировать динамически изменяющиеся сети.
/// </para>
/// </summary>
public sealed class TemporalGraph : IEquatable<TemporalGraph>
{
    /// <summary>
    /// Уникальный индекс графа в последовательности временных графов.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Интервал постоянства структуры.
    /// </summary>
    public TimeInterval Interval { get; }

    /// <summary>
    /// Коллекция связей, активных в этом временном интервале.
    /// </summary>
    public IReadOnlyList<Link> Links { get; }

    /// <summary>
    /// Все узлы сети, включая как активные, так и неактивные в данном интервале.
    /// </summary>
    public IReadOnlyList<string> AllNetworkNodes { get; }

    /// <summary>
    /// Все уникальные узлы, присутствующие в графе (вычисляемое свойство).
    /// Включает только узлы, участвующие в активных связях в данном интервале.
    /// </summary>
    public IReadOnlyList<string> ActiveNodes => Links
        .SelectMany(l => new[] { l.NodeA, l.NodeB })
        .Distinct()
        .OrderBy(n => n)
        .ToList()
        .AsReadOnly();

    /// <summary>
    /// Количество связей в графе.
    /// </summary>
    public int LinkCount => Links.Count();

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="TemporalGraph"/>.
    /// </summary>
    /// <param name="index">Индекс графа.</param>
    /// <param name="interval">Временной интервал активности.</param>
    /// <param name="links">Коллекция активных связей.</param>
    /// <param name="allNetworkNodes">Все узлы сети.</param>
    /// <exception cref="ArgumentNullException">Если links или allNetworkNodes равен null.</exception>
    public TemporalGraph(
        int index,
        TimeInterval interval,
        IEnumerable<Link> links,
        IEnumerable<string> allNetworkNodes)
    {
        Index = index;
        Interval = interval;
        Links = links?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(links));
        AllNetworkNodes = allNetworkNodes?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(allNetworkNodes));
    }

    /// <summary>
    /// Определяет равенство двух временных графов.
    /// </summary>
    /// <param name="other">Другой временной граф для сравнения.</param>
    /// <returns>true, если графы равны; иначе false.</returns>
    public bool Equals(TemporalGraph? other) =>
        other != null &&
        Index == other.Index &&
        Interval.Equals(other.Interval) &&
        Links.SequenceEqual(other.Links);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TemporalGraph);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        HashCode.Combine(Index, Interval, Links.Count);
}