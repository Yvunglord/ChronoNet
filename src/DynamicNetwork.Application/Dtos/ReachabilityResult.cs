using DynamicNetwork.Domain.Paths;

namespace DynamicNetwork.Application.Dtos;

public class ReachabilityResult
{
    public bool IsReachable { get; set; }
    public List<ReachabilityPathDto> AllPaths { get; set; } = new List<ReachabilityPathDto>();
    public int? ShortestPathLength { get; set; }
    public string Message { get; set; } = string.Empty;
}