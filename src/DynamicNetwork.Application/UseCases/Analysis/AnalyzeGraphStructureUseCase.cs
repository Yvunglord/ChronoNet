// Application/UseCases/Analysis/AnalyzeGraphStructureUseCase.cs
using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Application.Interfaces.UseCases.Analysis;
using DynamicNetwork.Domain.Analysis;
using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Domain.Services;

namespace DynamicNetwork.Application.UseCases.Analysis;

public class AnalyzeGraphStructureUseCase : IAnalyzeGraphStructureUseCase
{
    private readonly IGraphAnalysisDomainService _analyzer;

    public AnalyzeGraphStructureUseCase(IGraphAnalysisDomainService analyzer)
    {
        _analyzer = analyzer;
    }

    public AnalysisResult Execute(TemporalGraph graph)
    {
        var domainResult = _analyzer.Analyze(graph);

        return new AnalysisResult
        {
            VertexCount = domainResult.VertexCount,
            EdgeCount = domainResult.EdgeCount,
            IsConnected = domainResult.IsConnected,
            HasCycles = domainResult.HasCycles,
            Density = domainResult.Density,
            Diameter = domainResult.Diameter,
            DirectedLinksCount = domainResult.DirectedLinksCount,
            UndirectedLinksCount = domainResult.UndirectedLinksCount,
            StronglyConnectedComponentsCount = domainResult.StronglyConnectedComponentsCount,

            AdjacencyMatrixRows = ConvertMatrixToRows(domainResult.AdjacencyMatrix),
            IncidenceMatrixRows = ConvertMatrixToRows(domainResult.IncidenceMatrix),

            AdjacencyColumnHeaders = domainResult.AdjacencyMatrix.ColumnLabels.ToList(),
            IncidenceColumnHeaders = domainResult.IncidenceMatrix.ColumnLabels.ToList(),

            DegreeCentrality = domainResult.DegreeCentrality,
            BetweennessCentrality = domainResult.BetweennessCentrality,

            Message = $"Анализ завершён: {domainResult.VertexCount} вершин, {domainResult.EdgeCount} рёбер"
        };
    }

    private List<MatrixRowDto> ConvertMatrixToRows(Matrix<int> matrix)
    {
        var rows = new List<MatrixRowDto>();

        for (int i = 0; i < matrix.Rows; i++)
        {
            var rowDto = new MatrixRowDto
            {
                RowHeader = matrix.RowLabels[i],
                Cells = new List<MatrixCellDto>()
            };

            for (int j = 0; j < matrix.Columns; j++)
            {
                rowDto.Cells.Add(new MatrixCellDto
                {
                    Value = matrix.Values[i][j].ToString()
                });
            }

            rows.Add(rowDto);
        }

        return rows;
    }
}