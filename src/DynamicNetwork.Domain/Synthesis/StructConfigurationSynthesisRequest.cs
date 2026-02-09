using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Domain.Synthesis;

/// <summary>
/// Представляет запрос на синтез конфигурации структуры сети.
/// <para>
/// Содержит входные данные для алгоритма синтеза конфигурации.
/// </para>
/// </summary>
public class StructConfigurationSynthesisRequest
{
    /// <summary>
    /// Конфигурации узлов с входными данными и их временными интервалами.
    /// Ключ: конфигурация узла, Значение: временной интервал входных данных.
    /// </summary>
    public Dictionary<NodeConfiguration, TimeInterval> NodeInputs { get; init; } = new();

    /// <summary>
    /// Узлы, на которых должны появиться выходные данные.
    /// </summary>
    public List<NodeConfiguration> OutputNodes { get; init; } = new();

    /// <summary>
    /// Пользовательский временной интервал для синтеза конфигурации.
    /// В данной реализации будет определяться автоматически на основании
    /// знаний о входах и выходах
    /// </summary>
    public TimeInterval CustomInterval { get; init; } = TimeInterval.Empty;
}
