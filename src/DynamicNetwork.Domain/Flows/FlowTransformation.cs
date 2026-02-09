namespace DynamicNetwork.Domain.Flows;

/// <summary>
/// Представляет преобразование типа потока данных.
/// <para>
/// Трансформация описывает, как один тип данных преобразуется в другой.
/// Каждая трансформация требует наличия соответствующего процесса на узле.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// Последовательность трансформаций определяет путь преобразования данных
/// от исходного типа к целевому.
/// </para>
/// </remarks>
public sealed class FlowTransformation
{
    /// <summary>
    /// Исходный тип данных для преобразования.
    /// </summary>
    public string InputType { get; }

    /// <summary>
    /// Целевой тип данных после преобразования.
    /// </summary>
    public string OutputType { get; }

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="FlowTransformation"/>.
    /// </summary>
    /// <param name="inputType">Исходный тип данных.</param>
    /// <param name="outputType">Целевой тип данных.</param>
    /// <exception cref="ArgumentNullException">Выбрасывается, если inputType или outputType равен null.</exception>
    public FlowTransformation(string inputType, string outputType)
    {
        InputType = inputType ?? throw new ArgumentNullException(nameof(inputType));
        OutputType = outputType ?? throw new ArgumentNullException(nameof(outputType));
    }

    /// <summary>
    /// Определяет равенство двух трансформаций.
    /// </summary>
    /// <param name="other">Другая трансформация для сравнения.</param>
    /// <returns>true, если трансформации равны; иначе false.</returns>
    public bool Equals(FlowTransformation? other) =>
    other != null && InputType == other.InputType && OutputType == other.OutputType;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as FlowTransformation);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(InputType, OutputType);
}