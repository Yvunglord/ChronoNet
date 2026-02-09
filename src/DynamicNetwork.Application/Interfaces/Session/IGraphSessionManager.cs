using DynamicNetwork.Domain.Enums;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Interfaces.Session;

public interface IGraphSessionManager
{
    string CreateSession(string path);
    IReadOnlyList<TemporalGraph> GetGraphs(string sessionId);
    TemporalGraph UpdateLinkDirection(
        string sessionId,
        int graphIndex,
        string nodeA,
        string nodeB,
        LinkDirection newDirection);

    TemporalGraph UpdateLinkDirectionCycled(
        string sessionId,
        int graphIndex,
        string nodeA,
        string nodeB);

    void CloseSession(string sessionId);
}
