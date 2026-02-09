using DynamicNetwork.Domain.Paths;

namespace DynamicNetwork.Domain.Synthesis;

/// <summary>
/// Представляет план маршрутизации потока данных по сети.
/// <para>
/// Содержит информацию о пути потока, трансформациях данных и типах данных на узлах.
/// </para>
/// </summary>
public sealed class FlowRoutePlan
{
    /// <summary>
    /// Поток данных для маршрутизации.
    /// </summary>
    public required DataFlow Flow { get; init; }

    /// <summary>
    /// Путь достижимости для потока.
    /// </summary>
    public required ReachabilityPath Path { get; init; }

    /// <summary>
    /// Последовательность шагов трансформации потока.
    /// </summary>
    public required IReadOnlyList<FlowTransformationStep> Transformations { get; init; }

    /// <summary>
    /// Типы данных на каждом узле пути.
    /// Ключ - идентификатор узла, значение - тип данных на узле.
    /// </summary>
    public required IReadOnlyDictionary<string, string> NodeFlowTypes { get; init; }
}

