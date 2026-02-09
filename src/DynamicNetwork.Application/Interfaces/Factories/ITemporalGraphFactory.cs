using DynamicNetwork.Application.Dtos;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Interfaces.Factories;

public interface ITemporalGraphFactory
{
    IReadOnlyList<TemporalGraph> BuildGraphs(
        IEnumerable<LinkParsingDto> rawLinks);
}
