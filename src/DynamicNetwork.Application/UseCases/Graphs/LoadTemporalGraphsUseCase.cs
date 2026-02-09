using DynamicNetwork.Application.Interfaces.Factories;
using DynamicNetwork.Application.Interfaces.Ports;
using DynamicNetwork.Application.Interfaces.UseCases.Graphs;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.UseCases.Graphs;

public class LoadTemporalGraphsUseCase : ILoadTemporalGraphsUseCase
{
    private readonly ITemporalDataSourcePort _dataSource;
    private readonly ITemporalGraphFactory _factory;

    public LoadTemporalGraphsUseCase(
        ITemporalDataSourcePort dataSource,
        ITemporalGraphFactory factory)
    {
        _dataSource = dataSource;
        _factory = factory;
    }

    public IReadOnlyList<TemporalGraph> Execute(string sourcePath)
    {
        var rawLinks = _dataSource.LoadRawLinks(sourcePath);

        var graphs = _factory.BuildGraphs(rawLinks);

        return graphs;
    }
}
