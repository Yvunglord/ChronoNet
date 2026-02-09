using DynamicNetwork.Domain.Graph;
using DynamicNetwork.Application.Dtos;

namespace DynamicNetwork.Application.Interfaces.UseCases.Analysis;

public interface IAnalyzeGraphStructureUseCase
{
    AnalysisResult Execute(TemporalGraph graph);
}