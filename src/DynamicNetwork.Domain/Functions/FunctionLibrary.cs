namespace DynamicNetwork.Domain.Functions;

/// <summary>
/// Представляет библиотеку всех доступных функций в системе.
/// <para>
/// Библиотека функций содержит все типы процессов, транспортов и хранилищ,
/// которые могут быть использованы при синтезе конфигурации сети.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// Библиотека функций является справочником возможностей системы.
/// Алгоритм синтеза конфигурации использует эту библиотеку для определения,
/// какие функции могут быть применены для обработки конкретных типов данных.
/// </para>
/// </remarks>
public sealed class FunctionLibrary
{
    /// <summary>
    /// Все доступные типы процессов (функций обработки данных).
    /// </summary>
    public IReadOnlyCollection<ProcessType> Processes { get; }

    /// <summary>
    /// Все доступные типы транспорта (функций передачи данных).
    /// </summary>
    public IReadOnlyCollection<TransportType> Transports { get; }

    /// <summary>
    /// Все доступные типы хранилищ (функций хранения данных).
    /// </summary>
    public IReadOnlyCollection<StorageType> Storages { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="FunctionLibrary"/>.
    /// </summary>
    /// <param name="processes">Коллекция типов процессов.</param>
    /// <param name="transports">Коллекция типов транспорта.</param>
    /// <param name="storages">Коллекция типов хранилищ.</param>
    public FunctionLibrary(
        IEnumerable<ProcessType> processes,
        IEnumerable<TransportType> transports,
        IEnumerable<StorageType> storages
    )
    {
        Processes = processes.ToList().AsReadOnly();
        Transports = transports.ToList().AsReadOnly();
        Storages = storages.ToList().AsReadOnly();
    }

    /// <summary>
    /// Добавляет новые типы процессов в библиотеку, исключая дубликаты по идентификаторам.
    /// Возвращает новую неизменяемую библиотеку с объединенной коллекцией процессов.
    /// </summary>
    /// <param name="newProcesses">Новые типы процессов для добавления.</param>
    /// <returns>Новая библиотека функций с добавленными процессами.</returns>
    public FunctionLibrary AddProcesses(IEnumerable<ProcessType> newProcesses)
    {
        var existingIds = new HashSet<string>(Processes.Select(p => p.Id));
        var merged = Processes
            .Concat(newProcesses.Where(p => !existingIds.Contains(p.Id)))
            .ToList();

        return new FunctionLibrary(merged, Transports, Storages);
    }

    /// <summary>
    /// Удаляет типы процессов по их идентификаторам из библиотеки.
    /// Возвращает новую библиотеку без указанных процессов.
    /// </summary>
    /// <param name="processIds">Идентификаторы процессов для удаления.</param>
    /// <returns>Новая библиотека функций без удаленных процессов.</returns>
    public FunctionLibrary RemoveProcesses(IEnumerable<string> processIds)
    {
        var idsToRemove = new HashSet<string>(processIds);
        var filtered = Processes.Where(p => !idsToRemove.Contains(p.Id)).ToList();

        return new FunctionLibrary(filtered, Transports, Storages);
    }

    /// <summary>
    /// Добавляет новые типы транспорта в библиотеку, исключая дубликаты.
    /// Возвращает новую библиотеку с объединенной коллекцией транспортов.
    /// </summary>
    /// <param name="newTransports">Новые типы транспорта для добавления.</param>
    /// <returns>Новая библиотека функций с добавленными транспортами.</returns>
    public FunctionLibrary AddTransports(IEnumerable<TransportType> newTransports)
    {
        var existingIds = new HashSet<string>(Transports.Select(t => t.Id));
        var merged = Transports
            .Concat(newTransports.Where(t => !existingIds.Contains(t.Id)))
            .ToList();

        return new FunctionLibrary(Processes, merged, Storages);
    }

    /// <summary>
    /// Удаляет типы транспорта по их идентификаторам из библиотеки.
    /// Возвращает новую библиотеку без указанных транспортов.
    /// </summary>
    /// <param name="transportIds">Идентификаторы транспортов для удаления.</param>
    /// <returns>Новая библиотека функций без удаленных транспортов.</returns>
    public FunctionLibrary RemoveTransports(IEnumerable<string> transportIds)
    {
        var idsToRemove = new HashSet<string>(transportIds);
        var filtered = Transports.Where(t => !idsToRemove.Contains(t.Id)).ToList();

        return new FunctionLibrary(Processes, filtered, Storages);
    }

    /// <summary>
    /// Добавляет новые типы хранилищ в библиотеку, исключая дубликаты.
    /// Возвращает новую библиотеку с объединенной коллекцией хранилищ.
    /// </summary>
    /// <param name="newStorages">Новые типы хранилищ для добавления.</param>
    /// <returns>Новая библиотека функций с добавленными хранилищами.</returns>
    public FunctionLibrary AddStorages(IEnumerable<StorageType> newStorages)
    {
        var existingIds = new HashSet<string>(Storages.Select(s => s.Id));
        var merged = Storages
            .Concat(newStorages.Where(s => !existingIds.Contains(s.Id)))
            .ToList();

        return new FunctionLibrary(Processes, Transports, merged); // Исправлено: merged вместо Storages
    }

    /// <summary>
    /// Удаляет типы хранилищ по их идентификаторам из библиотеки.
    /// Возвращает новую библиотеку без указанных хранилищ.
    /// </summary>
    /// <param name="storageIds">Идентификаторы хранилищ для удаления.</param>
    /// <returns>Новая библиотека функций без удаленных хранилищ.</returns>
    public FunctionLibrary RemoveStorages(IEnumerable<string> storageIds)
    {
        var idsToRemove = new HashSet<string>(storageIds);
        var filtered = Storages.Where(s => !idsToRemove.Contains(s.Id)).ToList();

        return new FunctionLibrary(Processes, Transports, filtered);
    }

    /// <summary>
    /// Обновляет существующие типы процессов и добавляет новые в библиотеку.
    /// Если процесс с таким ID уже существует, он заменяется обновленной версией.
    /// Новые процессы добавляются в конец коллекции.
    /// </summary>
    /// <param name="updatedProcesses">Коллекция обновленных и/или новых процессов.</param>
    /// <returns>Новая библиотека функций с обновленными процессами.</returns>
    /// <exception cref="ArgumentNullException">Если updatedProcesses равен null.</exception>
    public FunctionLibrary UpdateProcesses(IEnumerable<ProcessType> updatedProcesses)
    {
        if (updatedProcesses == null) throw new ArgumentNullException(nameof(updatedProcesses));

        var updateDict = updatedProcesses.ToDictionary(p => p.Id);
        var existingIds = new HashSet<string>(Processes.Select(p => p.Id));

        var result = Processes
            .Select(p => updateDict.TryGetValue(p.Id, out var updated) ? updated : p)
            .Concat(updatedProcesses.Where(p => !existingIds.Contains(p.Id)))
            .ToList();

        return new FunctionLibrary(result, Transports, Storages);
    }

    /// <summary>
    /// Обновляет существующие типы транспорта и добавляет новые в библиотеку.
    /// Если транспорт с таким ID уже существует, он заменяется обновленной версией.
    /// Новые транспорты добавляются в конец коллекции.
    /// </summary>
    /// <param name="updatedTransports">Коллекция обновленных и/или новых транспортов.</param>
    /// <returns>Новая библиотека функций с обновленными транспортами.</returns>
    /// <exception cref="ArgumentNullException">Если updatedTransports равен null.</exception>
    public FunctionLibrary UpdateTransports(IEnumerable<TransportType> updatedTransports)
    {
        if (updatedTransports == null) throw new ArgumentNullException(nameof(updatedTransports));

        var updateDict = updatedTransports.ToDictionary(t => t.Id);
        var existingIds = new HashSet<string>(Transports.Select(t => t.Id));

        var result = Transports
            .Select(t => updateDict.TryGetValue(t.Id, out var updated) ? updated : t)
            .Concat(updatedTransports.Where(t => !existingIds.Contains(t.Id)))
            .ToList();

        return new FunctionLibrary(Processes, result, Storages);
    }

    /// <summary>
    /// Обновляет существующие типы хранилищ и добавляет новые в библиотеку.
    /// Если хранилище с таким ID уже существует, оно заменяется обновленной версией.
    /// Новые хранилища добавляются в конец коллекции.
    /// </summary>
    /// <param name="updatedStorages">Коллекция обновленных и/или новых хранилищ.</param>
    /// <returns>Новая библиотека функций с обновленными хранилищами.</returns>
    /// <exception cref="ArgumentNullException">Если updatedStorages равен null.</exception>
    public FunctionLibrary UpdateStorages(IEnumerable<StorageType> updatedStorages)
    {
        if (updatedStorages == null) throw new ArgumentNullException(nameof(updatedStorages));

        var updateDict = updatedStorages.ToDictionary(s => s.Id);
        var existingIds = new HashSet<string>(Storages.Select(s => s.Id));

        var result = Storages
            .Select(s => updateDict.TryGetValue(s.Id, out var updated) ? updated : s)
            .Concat(updatedStorages.Where(s => !existingIds.Contains(s.Id)))
            .ToList();

        return new FunctionLibrary(Processes, Transports, result);
    }

    /// <summary>
    /// Находит тип процесса по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор процесса для поиска.</param>
    /// <returns>Найденный ProcessType или null, если не найден.</returns>
    public ProcessType? GetProcessById(string id) =>
        Processes.FirstOrDefault(p => p.Id == id);

    /// <summary>
    /// Находит тип транспорта по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор транспорта для поиска.</param>
    /// <returns>Найденный TransportType или null, если не найден.</returns>
    public TransportType? GetTransportById(string id) =>
        Transports.FirstOrDefault(t => t.Id == id);

    /// <summary>
    /// Находит тип хранилища по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор хранилища для поиска.</param>
    /// <returns>Найденный StorageType или null, если не найден.</returns>
    public StorageType? GetStorageById(string id) =>
        Storages.FirstOrDefault(s => s.Id == id);
}