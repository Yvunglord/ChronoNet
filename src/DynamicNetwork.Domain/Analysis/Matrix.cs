namespace DynamicNetwork.Domain.Analysis;

/// <summary>
/// Доменный объект: матрица с метаданными для визуализации.
/// Иммутабельный объект с обязательной инициализацией через конструктор.
/// </summary>
public sealed class Matrix<T>
{
    /// <summary>
    /// Метки строк матрицы.
    /// </summary>
    public IReadOnlyList<string> RowLabels { get; }

    /// <summary>
    /// Метки столбцов матрицы.
    /// </summary>
    public IReadOnlyList<string> ColumnLabels { get; }

    /// <summary>
    /// Значения матрицы.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<T>> Values { get; }

    /// <summary>
    /// Количество строк в матрице.
    /// </summary>
    public int Rows => RowLabels.Count;

    /// <summary>
    /// Количество столбцов в матрице.
    /// </summary>
    public int Columns => ColumnLabels.Count;

    /// <summary>
    /// Создаёт матрицу с обязательной инициализацией всех свойств.
    /// Выполняет валидацию сразу при создании.
    /// </summary>
    /// <param name="rowLabels">Метки строк.</param>
    /// <param name="columnLabels">Метки столбцов.</param>
    /// <param name="values">Значения матрицы.</param>
    /// <exception cref="ArgumentNullException">Если любой из параметров равен null.</exception>
    /// <exception cref="InvalidOperationException">Если размеры меток и значений не совпадают.</exception>
    public Matrix(
        IReadOnlyList<string> rowLabels,
        IReadOnlyList<string> columnLabels,
        IReadOnlyList<IReadOnlyList<T>> values)
    {
        RowLabels = rowLabels ?? throw new ArgumentNullException(nameof(rowLabels));
        ColumnLabels = columnLabels ?? throw new ArgumentNullException(nameof(columnLabels));
        Values = values ?? throw new ArgumentNullException(nameof(values));

        // Валидация размеров
        if (RowLabels.Count != Values.Count)
            throw new InvalidOperationException(
                $"Row count mismatch: labels={RowLabels.Count}, values={Values.Count}");

        if (Values.Any() && ColumnLabels.Count != Values[0].Count)
            throw new InvalidOperationException(
                $"Column count mismatch: labels={ColumnLabels.Count}, values={Values[0].Count}");
    }

    /// <summary>
    /// Преобразовать в плоский список для визуализации (например, DataGrid).
    /// </summary>
    /// <returns>Перечисление ячеек матрицы.</returns>
    public IEnumerable<MatrixCell<T>> ToFlatList()
    {
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                yield return new MatrixCell<T>
                {
                    RowIndex = i,
                    ColumnIndex = j,
                    RowLabel = RowLabels[i],
                    ColumnLabel = ColumnLabels[j],
                    Value = Values[i][j]
                };
            }
        }
    }
}

/// <summary>
/// Представляет ячейку матрицы с метаданными для визуализации.
/// </summary>
public sealed class MatrixCell<T>
{
    /// <summary>
    /// Индекс строки ячейки.
    /// </summary>
    public int RowIndex { get; init; }

    /// <summary>
    /// Индекс столбца ячейки.
    /// </summary>
    public int ColumnIndex { get; init; }

    /// <summary>
    /// Метка строки ячейки.
    /// </summary>
    public string RowLabel { get; init; } = string.Empty;

    /// <summary>
    /// Метка столбца ячейки.
    /// </summary>
    public string ColumnLabel { get; init; } = string.Empty;

    /// <summary>
    /// Значение ячейки.
    /// </summary>
    public required T Value { get; init; }
}