using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Paths;

namespace DynamicNetwork.Domain.Services;

/// <summary>
/// Доменный сервис для поиска путей в графе.
/// <para>
/// Предоставляет методы для поиска всех возможных путей между узлами
/// с учетом временных интервалов и конфигураций.
/// Работает на трех уровнях: видимости, техническом и технологическом.
/// </para>
/// </summary>
public interface IPathFindingDomainService
{
    /// <summary>
    /// Находит все пути между исходным и целевым узлами в заданном временном окне.
    /// </summary>
    /// <param name="graphs">Последовательность временных графов.</param>
    /// <param name="configurations">Конфигурации структуры сети.</param>
    /// <param name="sourceNode">Исходный узел.</param>
    /// <param name="targetNode">Целевой узел.</param>
    /// <param name="timeWindow">Временное окно для поиска путей.</param>
    /// <returns>Список всех найденных путей достижимости.</returns>
    IReadOnlyList<ReachabilityPath> FindAllPaths(
        IReadOnlyList<TemporalGraph> graphs,
        IReadOnlyList<StructConfiguration> configurations,
        string sourceNode,
        string targetNode,
        TimeInterval timeWindow);
}
