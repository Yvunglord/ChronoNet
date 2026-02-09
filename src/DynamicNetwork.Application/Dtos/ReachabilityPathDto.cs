using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Dtos;

public sealed class ReachabilityPathDto
{
    public List<string> Path { get; set; } = new();
    public List<int> GraphIndices { get; set; } = new();
    public List<EdgeInfoDto> Edges { get; set; } = new();
    public int NodeCount { get; set; }
    public TimeInterval Interval { get; set; } = TimeInterval.Empty;
}

