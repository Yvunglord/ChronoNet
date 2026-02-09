using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Domain.Configuration;

/// <summary>
/// Представляет полную конфигурацию структуры сети на интервале постоянства структуры.
/// <para>
/// Структурная конфигурация включает все узлы и связи, а также их настройки
/// для конкретного временного интервала.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// Так как топология сети может меняться во времени, система работает с последовательностью
/// <see cref="StructConfiguration"/>, каждая из которых описывает состояние сети
/// в своем временном интервале.
/// </para>
/// <para>
/// Алгоритм синтеза конфигурации создает минимально необходимую конфигурацию для каждого
/// временного интервала на основе требований потоков данных.
/// </para>
/// </remarks>
public sealed class StructConfiguration
{
    /// <summary>
    /// Временной интервал, для которого актуальна эта конфигурация.
    /// </summary>
    public TimeInterval Interval { get; }

    /// <summary>
    /// Конфигурации всех узлов в этой структуре.
    /// </summary>
    public IReadOnlyCollection<NodeConfiguration> Nodes { get; }

    /// <summary>
    /// Конфигурации всех связей в этой структуре.
    /// </summary>
    public IReadOnlyCollection<LinkConfiguration> Links { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="StructConfiguration"/>.
    /// </summary>
    /// <param name="interval">Временной интервал.</param>
    /// <param name="nodes">Коллекция конфигураций узлов.</param>
    /// <param name="links">Коллекция конфигураций связей.</param>
    public StructConfiguration(
        TimeInterval interval,
        IEnumerable<NodeConfiguration> nodes,
        IEnumerable<LinkConfiguration> links
    )
    {
        Interval = interval;
        Nodes = nodes.ToList().AsReadOnly();
        Links = links.ToList().AsReadOnly();
    }

    /// <summary>
    /// Обновить узел по идентификатору (создаёт новый агрегат).
    /// </summary>
    /// <param name="nodeId">Идентификатор узла для обновления.</param>
    /// <param name="updateFunc">Функция обновления конфигурации узла.</param>
    /// <returns>Новая структурная конфигурация с обновленным узлом.</returns>
    /// <exception cref="InvalidOperationException">Если узел с указанным идентификатором не найден.</exception>
    public StructConfiguration WithUpdatedNode(string nodeId, Func<NodeConfiguration, NodeConfiguration> updateFunc)
    {
        var node = Nodes.FirstOrDefault(n => n.NodeId == nodeId)
            ?? throw new InvalidOperationException($"Node {nodeId} not found in configuration");

        var updatedNode = updateFunc(node);
        var updatedNodes = Nodes
            .Where(n => n.NodeId != nodeId)
            .Append(updatedNode)
            .ToList();

        return new StructConfiguration(Interval, updatedNodes, Links);
    }

    /// <summary>
    /// Обновить связь между узлами (создаёт новый агрегат).
    /// </summary>
    /// <param name="nodeA">Первый узел связи.</param>
    /// <param name="nodeB">Второй узел связи.</param>
    /// <param name="updateFunc">Функция обновления конфигурации связи.</param>
    /// <returns>Новая структурная конфигурация с обновленной связью.</returns>
    /// <exception cref="InvalidOperationException">Если связь между указанными узлами не найдена.</exception>
    public StructConfiguration WithUpdatedLink(string nodeA, string nodeB, Func<LinkConfiguration, LinkConfiguration> updateFunc)
    {
        var link = Links.FirstOrDefault(l =>
            (l.NodeA == nodeA && l.NodeB == nodeB) ||
            (l.NodeA == nodeB && l.NodeB == nodeA))
            ?? throw new InvalidOperationException($"Link between {nodeA} and {nodeB} not found");

        var updatedLink = updateFunc(link);
        var updatedLinks = Links
            .Where(l => l != link)
            .Append(updatedLink)
            .ToList();

        return new StructConfiguration(Interval, Nodes, updatedLinks);
    }
}