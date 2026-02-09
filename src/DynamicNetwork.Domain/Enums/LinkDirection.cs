namespace DynamicNetwork.Domain.Enums;

/// <summary>
/// Направление связи в графе.
/// </summary>
public enum LinkDirection
{
    /// <summary>
    /// Ненаправленная связь - данные могут передаваться в обоих направлениях.
    /// </summary>
    Undirected,

    /// <summary>
    /// Направленная связь справа - данные передаются только от NodeA к NodeB.
    /// </summary>
    Right,

    /// <summary>
    /// Направленная связь слева - данные передаются только от NodeB к NodeA.
    /// </summary>
    Left,

    /// <summary>
    /// Связь неактивна - передача данных невозможна. (Рудимент)
    /// </summary>
    None
}
