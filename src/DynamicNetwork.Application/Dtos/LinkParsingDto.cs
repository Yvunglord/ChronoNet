namespace DynamicNetwork.Application.Dtos;

public class LinkParsingDto
{
    public string NodeA { get; set; } = string.Empty;
    public string NodeB { get; set; } = string.Empty;
    public long Begin { get; set; } 
    public long End { get; set; }
}
