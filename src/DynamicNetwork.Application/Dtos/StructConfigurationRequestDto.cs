using DynamicNetwork.Domain.Configuration;
using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Dtos;

public class StructConfigurationRequestDto
{
    public Dictionary<NodeConfiguration, TimeInterval> NodeInputs { get; set; } = new();
    public List<NodeConfiguration> OutputNodes { get; set; } = new();
    public TimeInterval CustomInterval { get; set; } = TimeInterval.Empty;
}