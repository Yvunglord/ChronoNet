using DynamicNetwork.Domain.Enums;

namespace DynamicNetwork.Domain.Graph;

/// <summary>
/// Представляет связь между двумя узлами в графе с возможностью задания направления.
/// <para>
/// Связь является фундаментальной единицей топологии сети и определяет возможность
/// передачи данных между узлами.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// Направление связи влияет на маршрутизацию и поток данных:
/// - Undirected: данные могут передаваться в обоих направлениях
/// - Right: данные могут передаваться только от NodeA к NodeB
/// - Left: данные могут передаваться только от NodeB к NodeA
/// - None: связь неактивна. Параметр добавлен, для удобства манипуляций со связью
/// на технологическом уровне.
/// </para>
/// </remarks>
public sealed class Link
{
    /// <summary>
    /// Первый узел связи.
    /// </summary>
    public string NodeA { get; }

    /// <summary>
    /// Второй узел связи.
    /// </summary>
    public string NodeB { get; }

    /// <summary>
    /// Направление связи.
    /// </summary>
    public LinkDirection Direction { get; private set; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="Link"/>.
    /// </summary>
    /// <param name="nodeA">Первый узел.</param>
    /// <param name="nodeB">Второй узел.</param>
    /// <param name="direction">Направление связи. По умолчанию Undirected.</param>
    public Link(string nodeA, string nodeB,
        LinkDirection direction = LinkDirection.Undirected)
    {
        NodeA = nodeA;
        NodeB = nodeB;
        Direction = direction;
    }

    public Link WithDirection(LinkDirection newDirection) =>
        new Link(NodeA, NodeB, newDirection);

    public Link WithCycledDirection()
    {
        return WithDirection(Direction switch
        {
            LinkDirection.Undirected => LinkDirection.Right,
            LinkDirection.Right => LinkDirection.Left,
            LinkDirection.Left => LinkDirection.Undirected,
            _ => LinkDirection.Undirected
        });
    }

    /// <summary>
    /// Определяет равенство двух связей (по узлам, независимо от направления).
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        return (obj is Link other)
            && NodeA == other.NodeA
            && NodeB == other.NodeB;
    }

    /// <summary>
    /// Возвращает хэш-код связи (основан только на узлах).
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(NodeA, NodeB);
    }

    /// <summary>
    /// Возвращает строковое представление связи с указанием направления.
    /// </summary>
    /// <example>
    /// A ↔ B (Undirected)
    /// A → B (Right)
    /// A ← B (Left)
    /// A — B (None)
    /// </example>
    public override string ToString()
    {
        string arrow = Direction switch
        {
            LinkDirection.Undirected => "↔",
            LinkDirection.Right => "→",
            LinkDirection.Left => "←",
            _ => "—"
        };

        return $"{NodeA} {arrow} {NodeB}";
    }
}