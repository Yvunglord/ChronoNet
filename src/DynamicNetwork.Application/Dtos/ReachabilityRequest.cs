using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Dtos;

public class ReachabilityRequest
{
    public string SourceNode { get; set; } = string.Empty;
    public List<string> TargetNodes { get; set; } = new();
    public TimeInterval CustomInterval { get; set; }
    public long? DataSize { get; set; }
}