using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Domain.Synthesis;

/// <summary>
/// Представляет шаг трансформации потока данных в сети.
/// <para>
/// Может быть либо шагом обработки на узле, либо шагом передачи между узлами.
/// </para>
/// </summary>
public sealed class FlowTransformationStep
{
    /// <summary>
    /// Идентификатор узла для шага обработки.
    /// </summary>
    public string? NodeId { get; init; }

    /// <summary>
    /// Входной тип данных для шага обработки.
    /// </summary>
    public string? InputType { get; init; }

    /// <summary>
    /// Выходной тип данных для шага обработки.
    /// </summary>
    public string? OutputType { get; init; }

    /// <summary>
    /// Исходный узел для шага передачи.
    /// </summary>
    public string? SourceNode { get; init; }

    /// <summary>
    /// Целевой узел для шага передачи.
    /// </summary>
    public string? TargetNode { get; init; }

    /// <summary>
    /// Тип транспорта для шага передачи.
    /// </summary>
    public string? TransportType { get; init; }

    /// <summary>
    /// Временной интервал выполнения шага.
    /// </summary>
    public required TimeInterval Interval { get; init; }

    /// <summary>
    /// Объем потока данных для этого шага.
    /// </summary>
    public required double FlowVolume { get; init; }
}