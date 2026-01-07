namespace ChronoNet.Application.DTO;
public class RawJsonEdge
{
    public Dictionary<string, string> Attributes { get; set; } = new();
    public long Begin { get; set; }
    public long End { get; set; }
}