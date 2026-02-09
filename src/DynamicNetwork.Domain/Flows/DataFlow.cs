using DynamicNetwork.Domain.Flows;

/// <summary>
/// Представляет поток данных, который необходимо обработать в сети.
/// <para>
/// Поток данных имеет определенный объем и последовательность трансформаций,
/// которые необходимо применить к данным в процессе их обработки.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// Поток данных начинается с типа, указанного в <see cref="Id"/>,
/// затем последовательно проходит через все трансформации из <see cref="Transformations"/>.
/// </para>
/// <para>
/// Например, поток "Video4K" объемом 100 ГБ с трансформациями:
/// 1. Video4K → Video1080p
/// 2. Video1080p → Text
/// Будет преобразован из Video4K в Text через промежуточный тип Video1080p.
/// </para>
/// </remarks>
public sealed class DataFlow
{
    /// <summary>
    /// Уникальный идентификатор потока, также являющийся исходным типом данных.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Объем данных в потоке (в условных единицах, например, гигабайтах).
    /// </summary>
    public double Volume { get; }

    /// <summary>
    /// Последовательность трансформаций, которые необходимо применить к потоку.
    /// </summary>
    public IReadOnlyList<FlowTransformation> Transformations { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="DataFlow"/>.
    /// </summary>
    /// <param name="id">Идентификатор и исходный тип потока.</param>
    /// <param name="volume">Объем данных.</param>
    /// <param name="flowTransformations">Последовательность трансформаций.</param>
    /// <exception cref="ArgumentException">Выбрасывается, если volume ≤ 0.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если id = null или white space.</exception>
    public DataFlow(
        string id,
        double volume,
        IEnumerable<FlowTransformation> flowTransformations
    )
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or empty", nameof(id));
        if (volume <= 0)
            throw new ArgumentException("Volume must be positive", nameof(volume));

        Id = id;
        Volume = volume;
        Transformations = flowTransformations.ToList().AsReadOnly();
    }

    /// <summary>
    /// Создать копию потока с новым объёмом.
    /// </summary>
    public DataFlow WithVolume(double newVolume)
    {
        if (newVolume <= 0)
            throw new ArgumentException("Volume must be positive", nameof(newVolume));

        return new DataFlow(Id, newVolume, Transformations);
    }

    /// <summary>
    /// Создать копию потока с новыми трансформациями.
    /// </summary>
    public DataFlow WithTransformations(IEnumerable<FlowTransformation> newTransformations)
    {
        return new DataFlow(Id, Volume, newTransformations);
    }

    /// <summary>
    /// Добавить трансформацию в конец цепочки.
    /// </summary>
    public DataFlow AppendTransformation(FlowTransformation transformation)
    {
        var updated = Transformations.Append(transformation).ToList();
        return new DataFlow(Id, Volume, updated);
    }
}