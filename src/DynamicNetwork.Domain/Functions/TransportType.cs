namespace DynamicNetwork.Domain.Functions;

/// <summary>
/// Представляет тип транспорта для передачи данных между узлами.
/// <para>
/// Транспорт определяет характеристики передачи определенного типа данных
/// по связи между узлами.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// Транспорт характеризуется:
/// - Временем передачи (может зависеть от расстояния или других факторов)
/// - Емкостью (максимальный объем, который можно передать за одну операцию)
/// - Совместимостью с определенным типом данных
/// </para>
/// </remarks>
/// <example>
/// Пример транспорта:
/// - Id: "HighSpeedVideoChannel"
/// - FlowType: "Video4K"
/// - Time: 0.05 (секунд на передачу единицы данных)
/// - Capacity: 10.0 (гигабайт за одну операцию)
/// </example>
public sealed class TransportType
{
    /// <summary>
    /// Уникальный идентификатор типа транспорта.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Время передачи одной единицы данных (в секундах на единицу объема).
    /// </summary>
    public double Time { get; }

    /// <summary>
    /// Тип данных, который может передаваться этим транспортом.
    /// </summary>
    public string FlowType { get; }

    /// <summary>
    /// Емкость транспорта (максимальный объем за одну операцию передачи).
    /// </summary>
    public double Capacity { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="TransportType"/>.
    /// </summary>
    /// <param name="id">Идентификатор транспорта.</param>
    /// <param name="time">Время передачи единицы данных.</param>
    /// <param name="flowType">Тип передаваемых данных.</param>
    /// <param name="capacity">Емкость транспорта.</param>
    public TransportType(
        string id,
        double time,
        string flowType,
        double capacity
    )
    {
        Id = id;
        Time = time;
        FlowType = flowType;
        Capacity = capacity;
    }
}