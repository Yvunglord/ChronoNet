using DynamicNetwork.Domain.Analysis;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Domain.Services;

/// <summary>
/// Доменный сервис для анализа графов.
/// <para>
/// Предоставляет методы для вычисления различных характеристик и метрик графов.
/// </para>
/// </summary>
public interface IGraphAnalysisDomainService
{
    /// <summary>
    /// Анализирует временной граф и возвращает результат анализа.
    /// </summary>
    /// <param name="graph">Временной граф для анализа.</param>
    /// <returns>Результат анализа графа.</returns>
    GraphAnalysisResult Analyze(TemporalGraph graph);
}
