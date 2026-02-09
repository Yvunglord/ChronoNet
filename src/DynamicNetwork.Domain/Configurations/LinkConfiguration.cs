namespace DynamicNetwork.Domain.Configuration;

/// <summary>
/// Представляет конфигурацию связи между двумя узлами.
/// <para>
/// Связь может поддерживать различные типы транспорта для передачи данных.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// Связь имеет два набора транспортов:
/// - <see cref="EnabledTransports"/>: все типы транспорта, которые связь потенциально поддерживает
/// - <see cref="Transports"/>: транспорты, активные в текущей конфигурации
/// </para>
/// <para>
/// Например, одна физическая линия связи может поддерживать как "Ethernet", так и "FibreChannel",
/// но в конкретной конфигурации может быть активирован только один из них.
/// </para>
/// </remarks>
public sealed class LinkConfiguration
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
    /// Типы транспорта, которые связь потенциально поддерживает.
    /// </summary>
    public IReadOnlyCollection<string> EnabledTransports { get; }

    /// <summary>
    /// Типы транспорта, активные в текущей конфигурации связи.
    /// <para>
    /// Это подмножество <see cref="EnabledTransports"/>, которое фактически используется
    /// в синтезированной конфигурации.
    /// </para>
    /// </summary>
    public IReadOnlyCollection<string> ActiveTransports { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="LinkConfiguration"/>.
    /// </summary>
    /// <param name="nodeA">Первый узел.</param>
    /// <param name="nodeB">Второй узел.</param>
    /// <param name="enabledTransports">Доступные типы транспорта.</param>
    /// <param name="activeTransports">Активные транспорты.</param>
    public LinkConfiguration(
        string nodeA,
        string nodeB,
        IEnumerable<string> enabledTransports,
        IEnumerable<string> activeTransports
        )
    {
        NodeA = nodeA;
        NodeB = nodeB;
        EnabledTransports = enabledTransports.ToList().AsReadOnly();
        ActiveTransports = activeTransports.ToList().AsReadOnly();
    }

    /// <summary>
    /// Создает новую конфигурацию связи с обновленными активными транспортами.
    /// </summary>
    /// <param name="transports">Новые активные транспорты.</param>
    /// <returns>Новая конфигурация связи с обновленными активными транспортами.</returns>
    public LinkConfiguration WithActiveTransports(IEnumerable<string> transports)
    {
        var validated = transports.Where(t => EnabledTransports.Contains(t)).ToList();
        return new LinkConfiguration(NodeA, NodeB, EnabledTransports, validated);
    }
}