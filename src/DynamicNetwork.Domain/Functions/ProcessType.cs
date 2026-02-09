namespace DynamicNetwork.Domain.Functions;

/// <summary>
/// Представляет тип процесса (функции обработки данных).
/// <para>
/// Процесс преобразует данные из одного типа в другой.
/// Каждый процесс имеет определенную производительность, выраженную через
/// время обработки единицы данных и размер порции обработки.
/// </para>
/// </summary>
/// <example>
/// Пример процесса:
/// - Id: "VideoDownscaler"
/// - InputFlowType: "Video4K"
/// - OutputFlowType: "Video1080p"
/// - TimePerUnit: 0.1 (секунд на гигабайт)
/// - ChunkSize: 1.0 (гигабайт)
/// </example>
public sealed class ProcessType
{
    /// <summary>
    /// Уникальный идентификатор типа процесса.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Время обработки одной единицы данных (в секундах на единицу объема).
    /// </summary>
    public double TimePerUnit { get; }

    /// <summary>
    /// Тип данных на входе процесса.
    /// </summary>
    public string InputFlowType { get; }

    /// <summary>
    /// Тип данных на выходе процесса.
    /// </summary>
    public string OutputFlowType { get; }

    /// <summary>
    /// Размер порции данных для обработки (единица объема).
    /// </summary>
    public double ChunkSize { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="ProcessType"/>.
    /// </summary>
    /// <param name="id">Идентификатор процесса.</param>
    /// <param name="timePerUnit">Время обработки единицы данных.</param>
    /// <param name="inputFlowType">Входной тип данных.</param>
    /// <param name="outputFlowType">Выходной тип данных.</param>
    /// <param name="chunkSize">Размер порции обработки.</param>
    public ProcessType(
        string id,
        double timePerUnit,
        string inputFlowType,
        string outputFlowType,
        double chunkSize
    )
    {
        Id = id;
        TimePerUnit = timePerUnit;
        InputFlowType = inputFlowType;
        OutputFlowType = outputFlowType;
        ChunkSize = chunkSize;
    }
}