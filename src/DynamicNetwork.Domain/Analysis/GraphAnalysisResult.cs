namespace DynamicNetwork.Domain.Analysis;

// <summary>
/// Представляет результат анализа графа с метриками и структурной информацией.
/// <para>
/// Содержит основные характеристики графа, такие как связность, наличие циклов,
/// плотность, диаметр, а также матрицы смежности и инцидентности.
/// </para>
/// </summary>
public sealed class GraphAnalysisResult
{
    /// <summary>
    /// Количество вершин (узлов) в графе.
    /// </summary>
    public required int VertexCount { get; init; }

    /// <summary>
    /// Количество ребер (связей) в графе.
    /// </summary>
    public required int EdgeCount { get; init; }

    /// <summary>
    /// Указывает, является ли граф связным.
    /// </summary>
    public required bool IsConnected { get; init; }

    /// <summary>
    /// Указывает, содержит ли граф циклы.
    /// </summary>
    public required bool HasCycles { get; init; }

    /// <summary>
    /// Плотность графа (отношение фактического числа ребер к максимально возможному).
    /// </summary>
    public required double Density { get; init; }

    /// <summary>
    /// Диаметр графа (максимальное расстояние между любыми двумя вершинами).
    /// </summary>
    public required int Diameter { get; init; }

    /// <summary>
    /// Количество ненаправленных связей в графе.
    /// </summary>
    public required int UndirectedLinksCount { get; init; }

    /// <summary>
    /// Количество направленных связей в графе.
    /// </summary>
    public required int DirectedLinksCount { get; init; }

    /// <summary>
    /// Матрица смежности графа.
    /// </summary>
    public required Matrix<int> AdjacencyMatrix { get; init; }

    /// <summary>
    /// Матрица инцидентности графа.
    /// </summary>
    public required Matrix<int> IncidenceMatrix { get; init; }

    /// <summary>
    /// Центральность по степени для каждого узла.
    /// </summary>
    public required Dictionary<string, double> DegreeCentrality { get; init; }

    /// <summary>
    /// Посредническая центральность для каждого узла.
    /// </summary>
    public required Dictionary<string, double> BetweennessCentrality { get; init; }

    /// <summary>
    /// Количество сильно связных компонент в направленном графе.
    /// </summary>
    public required int StronglyConnectedComponentsCount { get; init; }
}