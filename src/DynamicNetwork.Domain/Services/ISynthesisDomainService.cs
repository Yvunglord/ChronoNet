using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Functions;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Paths;
using DynamicNetwork.Domain.Synthesis;

namespace DynamicNetwork.Domain.Services;

/// <summary>
/// Доменный сервис для синтеза конфигураций сети.
/// <para>
/// Предоставляет методы для синтеза минимальных конфигураций сети
/// на основе требований потоков данных и доступных функций.
/// </para>
/// </summary>
public interface ISynthesisDomainService
{
    /// <summary>
    /// Синтезирует все необходимые конфигурации структуры сети.
    /// </summary>
    /// <param name="request">Запрос на синтез конфигурации.</param>
    /// <param name="graphs">Последовательность временных графов.</param>
    /// <param name="flows">Потоки данных для обработки.</param>
    /// <param name="functionLibrary">Библиотека доступных функций.</param>
    /// <param name="baseConfigurations">Базовые конфигурации сети.</param>
    /// <param name="reachablePaths">Доступные пути в сети.</param>
    /// <returns>Список синтезированных конфигураций структуры сети.</returns>
    IReadOnlyList<StructConfiguration> SynthesizeAll(
        StructConfigurationSynthesisRequest request,
        IReadOnlyList<TemporalGraph> graphs,
        IReadOnlyList<DataFlow> flows,
        FunctionLibrary functionLibrary,
        IReadOnlyList<StructConfiguration> baseConfigurations,
        IReadOnlyList<ReachabilityPath> reachablePaths);
}
