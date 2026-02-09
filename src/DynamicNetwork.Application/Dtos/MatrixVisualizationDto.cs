using DynamicNetwork.Domain.Analysis;

namespace DynamicNetwork.Application.Dtos;

public sealed class MatrixVisualizationDto<T>
{
    public required IReadOnlyList<string> RowHeaders { get; init; }
    public required IReadOnlyList<string> ColumnHeaders { get; init; }
    public required IReadOnlyList<IReadOnlyList<T>> Cells { get; init; }

    public static MatrixVisualizationDto<T> FromDomain(Matrix<T> matrix)
    {
        return new MatrixVisualizationDto<T>
        {
            RowHeaders = matrix.RowLabels,
            ColumnHeaders = matrix.ColumnLabels,
            Cells = matrix.Values
        };
    }
}

public sealed class MatrixRowDto
{
    public required string RowHeader { get; set; }
    public required List<MatrixCellDto> Cells { get; set; }
}

public sealed class MatrixCellDto
{
    public required string Value { get; set; }
}