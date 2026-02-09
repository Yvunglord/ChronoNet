namespace DynamicNetwork.Domain.Configuration;

/// <summary>
/// Представляет конфигурацию узла (ноды) в сети.
/// <para>
/// Узел может выполнять различные процессы, принимать и выдавать определенные типы данных,
/// а также хранить данные в различных типах хранилищ.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// Узел имеет два набора процессов:
/// - <see cref="EnabledProcesses"/>: все процессы, которые узел потенциально может выполнять
/// - <see cref="Processes"/>: процессы, которые активны в текущей конфигурации
/// </para>
/// <para>
/// Разделение на Enabled и активные процессы позволяет синтезировать минимальную конфигурацию,
/// включая только необходимые для конкретного сценария процессы, а хранить обе эти коллекции
/// прямо здесь чрезвычайно удобно.
/// </para>
/// </remarks>
public sealed class NodeConfiguration
{
    /// <summary>
    /// Уникальный идентификатор узла.
    /// </summary>
    public string NodeId { get; }

    /// <summary>
    /// Процессы, которые узел потенциально может выполнять.
    /// </summary>
    public IReadOnlyCollection<string> EnabledProcesses { get; }

    /// <summary>
    /// Типы данных, которые поступают на узел из внешнего мира.
    /// Неактуально для промежуточных узлов.
    /// </summary>
    public IReadOnlyCollection<string> Inputs { get; }

    /// <summary>
    /// Типы данных, которые должны появиться на узле в результате подбора конфигурации.
    /// Неактуально для промежуточных узлов.
    /// </summary>
    public IReadOnlyCollection<string> Outputs { get; }

    /// <summary>
    /// Емкости различных типов хранилищ, настроенных на узле.
    /// Ключ - идентификатор типа хранилища, значение - емкость.
    /// </summary>
    public IReadOnlyDictionary<string, double> StorageCapacities { get; }

    /// <summary>
    /// Процессы, активные в текущей конфигурации узла.
    /// <para>
    /// Это подмножество <see cref="EnabledProcesses"/>, которое фактически используется
    /// в синтезированной конфигурации.
    /// </para>
    /// </summary>
    public IReadOnlyCollection<string> ActiveProcesses { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="NodeConfiguration"/>.
    /// </summary>
    /// <param name="nodeId">Идентификатор узла.</param>
    /// <param name="enabledProcesses">Доступные процессы.</param>
    /// <param name="inputs">Входные типы данных.</param>
    /// <param name="outputs">Выходные типы данных.</param>
    /// <param name="storageCapacities">Емкости хранилищ.</param>
    /// <param name="activeProcesses">Активные процессы.</param>
    public NodeConfiguration(
        string nodeId,
        IEnumerable<string> enabledProcesses,
        IEnumerable<string> inputs,
        IEnumerable<string> outputs,
        IDictionary<string, double> storageCapacities,
        IEnumerable<string> activeProcesses
    )
    {
        NodeId = nodeId;
        EnabledProcesses = enabledProcesses.ToList().AsReadOnly();
        Inputs = inputs.ToList().AsReadOnly();
        Outputs = outputs.ToList().AsReadOnly();
        StorageCapacities = new Dictionary<string, double>(storageCapacities);
        ActiveProcesses = activeProcesses.ToList().AsReadOnly();
    }

    /// <summary>
    /// Создает новую конфигурацию узла с обновленными активными процессами.
    /// </summary>
    /// <param name="processes">Новые активные процессы.</param>
    /// <returns>Новая конфигурация узла с обновленными активными процессами.</returns>
    public NodeConfiguration WithActiveProcesses(IEnumerable<string> processes)
    {
        var validated = processes.Where(p => EnabledProcesses.Contains(p)).ToList();
        return new NodeConfiguration(
            NodeId, EnabledProcesses, Inputs, Outputs, StorageCapacities.ToDictionary(), validated);
    }
}