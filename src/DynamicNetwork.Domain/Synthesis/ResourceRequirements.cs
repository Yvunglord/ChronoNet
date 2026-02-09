using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Domain.Synthesis;

/// <summary>
/// Представляет требования к ресурсам сети для обработки потоков данных.
/// <para>
/// Содержит требования к процессам на узлах, транспортам на связях
/// и хранилищам на узлах с разбивкой по временным интервалам.
/// </para>
/// </summary>
public sealed class ResourceRequirements
{
    /// <summary>
    /// Требования к процессам на узлах.
    /// Ключ: идентификатор узла → Ключ: временной интервал → Значение: набор требуемых процессов.
    /// </summary>
    public required IReadOnlyDictionary<string, IReadOnlyDictionary<TimeInterval, IReadOnlySet<string>>>
        NodeProcessRequirements
    { get; init; }

    /// <summary>
    /// Требования к транспортам на связях.
    /// Ключ: пара узлов (исходный, целевой) → Ключ: временной интервал → Значение: набор требуемых транспортов.
    /// </summary>
    public required IReadOnlyDictionary<(string Source, string Target),
        IReadOnlyDictionary<TimeInterval, IReadOnlySet<string>>>
        LinkTransportRequirements
    { get; init; }

    /// <summary>
    /// Требования к хранилищам на узлах.
    /// Ключ: идентификатор узла → Ключ: временной интервал → Ключ: тип хранилища → Значение: требуемая емкость.
    /// </summary>
    public required IReadOnlyDictionary<string,
        IReadOnlyDictionary<TimeInterval, IReadOnlyDictionary<string, double>>>
        NodeStorageRequirements
    { get; init; }
}