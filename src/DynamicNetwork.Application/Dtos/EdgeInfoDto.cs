using DynamicNetwork.Domain.Graph;

namespace DynamicNetwork.Application.Dtos;

public class EdgeInfoDto
{
    public string FromNode { get; set; } = string.Empty;
    public string ToNode { get; set; } = string.Empty;
    public int GraphIndex { get; set; }
    public Link? Link { get; set; }
}
