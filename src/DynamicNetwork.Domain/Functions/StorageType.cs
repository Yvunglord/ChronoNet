namespace DynamicNetwork.Domain.Functions;

/// <summary>
/// Представляет тип хранилища для данных.
/// <para>
/// Хранилище определяет, какие типы данных могут в нем храниться.
/// </para>
/// </summary>
/// <example>
/// Примеры хранилищ:
/// - "FastSSD": ["Video4K", "Video1080p", "Audio"]
/// - "ArchiveHDD": ["Text", "Logs", "Backups"]
/// - "MemoryCache": ["Video4K", "Video1080p"]
/// </example>
public sealed class StorageType
{
    /// <summary>
    /// Уникальный идентификатор типа хранилища.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Типы данных, которые могут храниться в этом хранилище.
    /// </summary>
    public IReadOnlyCollection<string> AllowedFlowTypes { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="StorageType"/>.
    /// </summary>
    /// <param name="id">Идентификатор хранилища.</param>
    /// <param name="allowedFlowTypes">Разрешенные типы данных.</param>
    public StorageType(string id, IEnumerable<string> allowedFlowTypes)
    {
        Id = id;
        AllowedFlowTypes = allowedFlowTypes.ToList().AsReadOnly();
    }
}